using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class WFC3D : MonoBehaviour
{
    // Grid dimensions
    public int xSize, ySize, zSize;

    // Generates grid
    private GridManager gridManager;

    //Generates adjacency rules from sample
    private SampleManager3D sampleManager;

    // All module names
    private List<Tuple<string, int>> moduleTypes;

    List<Tuple<string, int>> edgeModuleTypes;
    private Dictionary<string, Tuple<GameObject, int>> gameObjects;

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
    private List<string> errorStates;
    private List<History3D> history;

    // Use this for initialization
    void Start()
    {
        gridManager = new GridManager(xSize, ySize, zSize);
        blocks = gridManager.CreateGrid();

        sampleManager = new SampleManager3D();

        rules = sampleManager.GenerateRulesFromSamples();
        gameObjects = sampleManager.GetObjects();
        moduleTypes = new List<Tuple<string, int>>();

        foreach (var rule in rules)
            moduleTypes.Add(new(rule.Key, gameObjects[rule.Key[0..^1]].Item2));

        
        modules = new Module[xSize, ySize, zSize];
        newModules = new Module[xSize, ySize, zSize];

        edgeModuleTypes = moduleTypes.Where(tuple => !tuple.Item1.Contains("ground")).ToList();
        
        foreach (var block in blocks)
        {
            int x = block.x;
            int y = block.y;
            int z = block.z;

            if ((x == 0 || x == blocks.GetLength(0) - 1 || z == 0 || z == blocks.GetLength(2) - 1) && y == 0)
                modules[x, y, z] = new Module(new Vector3Int(x, y, z), edgeModuleTypes, true);
            else
                modules[x, y, z] = new Module(new Vector3Int(x, y, z), moduleTypes, false);
        }

        lowEntropyList = new List<Vector3Int>();

        errorStates = new List<string>();
        history = new List<History3D>();

        modules[0, 0, 0].CollapseTo("streetc1");
        UpdateValids();


    }

    // Update is called once per frame
    void Update()
    {
        if (!CheckFullyCollapsed())
        {
            UpdateEntropy();

            if (lowEntropyList.Count <= 0)
                return;

            System.Random random = new System.Random();
            int index = random.Next(0, lowEntropyList.Count);
            Vector3Int currentCell = lowEntropyList[index];

            bool fertig = false;

            while (modules[currentCell.x, currentCell.y, currentCell.z].GetValidTypes().Count > 0 && !fertig)
            {
                modules[currentCell.x, currentCell.y, currentCell.z].Collapse();

                if (errorStates.Contains(CurrentState()))
                    modules[currentCell.x, currentCell.y, currentCell.z].RemoveType(modules[currentCell.x, currentCell.y, currentCell.z].GetTileType());

                else
                {
                    history.Add(new History3D(CurrentState(), new(currentCell, modules[currentCell.x, currentCell.y, currentCell.z].GetTileType())));
                    fertig = true;
                }
            }

            if (!fertig)
            {
                modules[currentCell.x, currentCell.y, currentCell.z].ResetModule();
                errorStates.Add(CurrentState());

                List<Vector3Int> cellAdjacents = GetCellAdjacents(currentCell);
                History3D lastCollapsed = history
                    .Where(history => cellAdjacents.Contains(history.Step.Item1))
                    .LastOrDefault();

                int lastCollapsedIndex = history.IndexOf(lastCollapsed);
                LoadState(history[lastCollapsedIndex].CurrentState);
                errorStates.Add(CurrentState());
                foreach (Module module in modules)
                {
                    if (cellAdjacents.Contains(module.GetGridPosition()))
                    {
                        module.ResetModule();
                    }
                }

                history.RemoveRange(lastCollapsedIndex, history.Count - lastCollapsedIndex);
            }

            UpdateValids();

            foreach (var module in modules)
                if (module.collapsed && module.model == null)
                    module.SetObject(gameObjects[module.GetTileType()[0..^1]].Item1);
        }
    }

    // Gets a list of blocks with the lowest entropy (number of valid modules)
    private void UpdateEntropy()
    {
        int lowest = int.MaxValue;
        lowEntropyList.Clear();

        foreach (var module in modules)
        {
            if ((module.collapsed == true) || (module.GetValidTypes().Count > lowest))
                continue;

            if (module.GetValidTypes().Count < lowest)
            {
                lowest = module.GetValidTypes().Count;
                lowEntropyList.Clear();
                lowEntropyList.Add(module.gridPosition);
            }

            else if (module.GetValidTypes().Count == lowest)
                lowEntropyList.Add(module.gridPosition);
        }
    }

    // Gets a list of valid modules for a direction
    public List<string> GetValidsForDirection(string type, Dir dir)
    {
        return rules[type][dir];
    }

    private string CurrentState()
    {
        string state = "";
        string separator = "-";
        foreach (var module in modules)
        {
            if (!module.IsCollapsed())
                state += "x" + separator;
            else
                state += moduleTypes.FindIndex(tuple => tuple.Item1 == module.GetTileType()).ToString() + separator;
        }

        state = state.Remove(state.Length - 1);
        return state;
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

            if (CheckEdgeCollapsed() || (modules[x, y, z].IsEdge() && !CheckEdgeCollapsed()))
            {
                if (modules[x, y, z].collapsed)
                    newModules[x, y, z] = modules[x, y, z];

                else
                {
                    List<string> options = new();
                    if (!CheckEdgeCollapsed())
                    {
                        options = edgeModuleTypes.Select(tuple => tuple.Item1).ToList();
                    }
                    else
                    {
                        options = moduleTypes.Select(tuple => tuple.Item1).ToList();
                    }

                    if (x > 0)
                    {
                        valids.Clear();
                        if (modules[x - 1, y, z].collapsed)
                            valids.AddRange(GetValidsForDirection(modules[x - 1, y, z].GetTileType(), Dir.Right));

                        else
                            valids = options;

                        valids = valids.Distinct().ToList();
                        options = options.Intersect(valids).ToList();
                    }

                    if (x < blocks.GetLength(0) - 1)
                    {
                        valids.Clear();
                        if (modules[x + 1, y, z].collapsed)
                            valids.AddRange(GetValidsForDirection(modules[x + 1, y, z].GetTileType(), Dir.Left));

                        else
                            valids = options;

                        valids = valids.Distinct().ToList();
                        options = options.Intersect(valids).ToList();
                    }

                    if (y > 0)
                    {
                        valids.Clear();
                        if (modules[x, y - 1, z].collapsed)
                            valids.AddRange(GetValidsForDirection(modules[x, y - 1, z].GetTileType(), Dir.Up));

                        else
                            valids = options;

                        valids = valids.Distinct().ToList();
                        options = options.Intersect(valids).ToList();
                    }

                    if (y < blocks.GetLength(1) - 1)
                    {
                        valids.Clear();
                        if (modules[x, y + 1, z].collapsed)
                            valids.AddRange(GetValidsForDirection(modules[x, y + 1, z].GetTileType(), Dir.Down));

                        else
                            valids = options;

                        valids = valids.Distinct().ToList();
                        options = options.Intersect(valids).ToList();
                    }

                    if (z > 0)
                    {
                        valids.Clear();
                        if (modules[x, y, z - 1].collapsed)
                            valids.AddRange(GetValidsForDirection(modules[x, y, z - 1].GetTileType(), Dir.Forward));

                        else
                            valids = options;

                        valids = valids.Distinct().ToList();
                        options = options.Intersect(valids).ToList();
                    }

                    if (z < blocks.GetLength(2) - 1)
                    {
                        valids.Clear();
                        if (modules[x, y, z + 1].collapsed)
                            valids.AddRange(GetValidsForDirection(modules[x, y, z + 1].GetTileType(), Dir.Back));

                        else
                            valids = options;

                        valids = valids.Distinct().ToList();
                        options = options.Intersect(valids).ToList();
                    }

                    newModules[x, y, z].SetValidTypes(options);
                }
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

    private bool CheckEdgeCollapsed()
    {
        foreach (var module in modules)
            if (module.IsEdge() && !module.collapsed)
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

    private List<Vector3Int> GetCellAdjacents(Vector3Int cell)
    {
        List<Vector3Int> adjacents = new();
        int x = cell.x;
        int y = cell.y;
        int z = cell.z;

        if (x > 0)
            if (modules[x - 1, y, z].IsCollapsed())
                adjacents.Add(modules[x - 1, y, z].GetGridPosition());

        if (x < blocks.GetLength(0) - 1)
            if (modules[x + 1, y, z].IsCollapsed())
                adjacents.Add(modules[x + 1, y, z].GetGridPosition());

        if (z > 0)
            if (modules[x, y, z - 1].IsCollapsed())
                adjacents.Add(modules[x, y, z - 1].GetGridPosition());

        if (z < blocks.GetLength(2) - 1)
            if (modules[x, y, z + 1].IsCollapsed())
                adjacents.Add(modules[x, y, z + 1].GetGridPosition());

        return adjacents;
    }

    private void LoadState(string state)
    {
        string[] states = state.Split("-");
        int index = 0;
        foreach (var module in modules)
        {
            if (states[index].Equals("x"))
                module.ResetModule();
            else
                module.CollapseToType(moduleTypes[int.Parse(states[index])].Item1);

            index++;
        }
    }
}