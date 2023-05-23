using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class ClassicWFC3D : MonoBehaviour
{
    // Grid dimensions
    public int xSize, ySize, zSize;

    // Generates grid
    private GridManager gridManager;

    //Generates adjacency rules from sample
    private SampleManager3D sampleManager;

    // All module names
    private List<string> moduleTypes;
    private Dictionary<string, GameObject> gameObjects;

    // Module adjacency rules for each direction
    private Dictionary<string, Dictionary<Dir, List<string>>> rules;

    // Grid
    private Vector3Int[,,] blocks;

    // Modules
    private Module[,,] modules;
    private Module[,,] newModules;

    // Blocks (coordinates) with the lowest entropy
    private List<Vector3Int> lowEntropyList;

    // Backtraking
    private List<(Vector3Int, string)> steps;
    private Dictionary<Vector2Int, int> history;
    private int maxResets = 2;
    private int resets = 0;

    // Use this for initialization
    void Start()
    {
        gridManager = new GridManager(xSize, ySize, zSize);
        blocks = gridManager.CreateGrid();

        sampleManager = new SampleManager3D();
        gameObjects = sampleManager.GetObjects();

        moduleTypes = new List<string>();
        foreach (var item in gameObjects)
            moduleTypes.Add(item.Key);

        rules = sampleManager.GenerateFromSource();

        modules = new Module[xSize, ySize, zSize];
        newModules = new Module[xSize, ySize, zSize];

        foreach (var block in blocks)
        {
            int x = block.x;
            int y = block.y;
            int z = block.z;

            modules[x, y, z] = new Module(new Vector3Int(x, y, z), moduleTypes);
        }

        lowEntropyList = new List<Vector3Int>();

        steps = new List<(Vector3Int, string)>();
        history = new Dictionary<Vector2Int, int>();

        CollapseEdges();
        UpdateValids();

    }

    // Update is called once per frame
    void Update()
    {
        if (CheckFullyCollapsed() == false)
        {
            UpdateEntropy();

            if (lowEntropyList.Count <= 0)
                return;

            System.Random random = new System.Random();
            int index = random.Next(0, lowEntropyList.Count);
            Vector3Int cell = lowEntropyList[index];

            if (modules[cell.x, cell.y, cell.z].validTypes.Count > 0)
            {
                modules[cell.x, cell.y, cell.z].Collapse();
                steps.Add(new(cell, modules[cell.x, cell.y, cell.z].type));
            }
            else
            {
                if (steps.Count > 0)
                {
                    while (!modules[steps[^1].Item1.x, steps[^1].Item1.y, steps[^1].Item1.z].CollapseOther(steps[^1].Item2))
                    {
                        steps.RemoveAt(steps.Count - 1);
                    }
                }
            }

            UpdateValids();

            foreach (var module in modules)
                if (module.collapsed && module.model == null)
                    module.SetObject(gameObjects[module.type]);
        }
    }

    // Gets a list of blocks with the lowest entropy (number of valid modules)
    private void UpdateEntropy()
    {
        int lowest = int.MaxValue;
        lowEntropyList.Clear();

        foreach (var module in modules)
        {
            if ((module.collapsed == true) || (module.validTypes.Count > lowest))
                continue;

            if (module.validTypes.Count < lowest)
            {
                lowest = module.validTypes.Count;
                lowEntropyList.Clear();
                lowEntropyList.Add(module.gridPosition);
            }

            else if (module.validTypes.Count == lowest)
                lowEntropyList.Add(module.gridPosition);
        }
    }

    // Gets a list of valid modules for a direction
    public List<string> GetValidsForDirection(string type, Dir dir)
    {
        return rules[type][dir];
    }

    // Sets valid modules for each block by checking all directions
    private void UpdateValids()
    {
        Array.Copy(modules, newModules, modules.Length);
        List<string> valids = new();

        foreach (var block in blocks)
        {
            int x = block.x;
            int y = block.y;
            int z = block.z;

            if (modules[x, y, z].collapsed)
                newModules[x, y, z] = modules[x, y, z];

            else
            {
                List<string> options = moduleTypes;
                if (x > 0)
                {
                    valids.Clear();
                    if (modules[x - 1, y, z].collapsed)
                        valids.AddRange(GetValidsForDirection(modules[x - 1, y, z].type, Dir.Right));

                    else
                        valids = options;

                    valids = valids.Distinct().ToList();
                    options = options.Intersect(valids).ToList();
                }

                if (x < blocks.GetLength(0) - 1)
                {
                    valids.Clear();
                    if (modules[x + 1, y, z].collapsed)
                        valids.AddRange(GetValidsForDirection(modules[x + 1, y, z].type, Dir.Left));

                    else
                        valids = options;

                    valids = valids.Distinct().ToList();
                    options = options.Intersect(valids).ToList();
                }

                if (y > 0)
                {
                    valids.Clear();
                    if (modules[x, y - 1, z].collapsed)
                        valids.AddRange(GetValidsForDirection(modules[x, y - 1, z].type, Dir.Up));

                    else
                        valids = options;

                    valids = valids.Distinct().ToList();
                    options = options.Intersect(valids).ToList();
                }

                if (y < blocks.GetLength(1) - 1)
                {
                    valids.Clear();
                    if (modules[x, y + 1, z].collapsed)
                        valids.AddRange(GetValidsForDirection(modules[x, y + 1, z].type, Dir.Down));

                    else
                        valids = options;

                    valids = valids.Distinct().ToList();
                    options = options.Intersect(valids).ToList();
                }

                if (z > 0)
                {
                    valids.Clear();
                    if (modules[x, y, z - 1].collapsed)
                        valids.AddRange(GetValidsForDirection(modules[x, y, z - 1].type, Dir.Forward));

                    else
                        valids = options;

                    valids = valids.Distinct().ToList();
                    options = options.Intersect(valids).ToList();
                }

                if (z < blocks.GetLength(2) - 1)
                {
                    valids.Clear();
                    if (modules[x, y, z + 1].collapsed)
                        valids.AddRange(GetValidsForDirection(modules[x, y, z + 1].type, Dir.Back));

                    else
                        valids = options;

                    valids = valids.Distinct().ToList();
                    options = options.Intersect(valids).ToList();
                }

                newModules[x, y, z].validTypes = options;
            }
        }

        modules = newModules;
    }

    private bool CheckFullyCollapsed()
    {
        foreach (var module in modules)
            if (!module.collapsed)
                return false;

        return true;
    }

    public void ResetGrid()
    {
        foreach (var module in modules)
        {
            module.ResetModule();
        }
    }

    private void Backtrack(int backtrackAmount)
    {
        if (backtrackAmount > steps.Count)
        {
            ResetGrid();
            history.Clear();
            resets++;

            if (resets >= maxResets)
            {
                foreach (var module in modules)
                    module.ResetModule();

                resets = 0;
            }
        }
    }

    private void CollapseEdges()
    {
        d
        foreach (var block in blocks)
        {
            int x = block.x;
            int y = block.y;
            int z = block.z;

            if ((x == 0 || x == blocks.GetLength(0) - 1 || z == 0 || z == blocks.GetLength(2) - 1) && y == 0)
            {
                modules[x, y, z].CollapseTo("EMPTY");
            }
        }
    }
}