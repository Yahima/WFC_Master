using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class WFC2D : MonoBehaviour
{
    public string path, xmlPath;
    public int tileSize;
    public int cols, rows;

    private RectTransform container;

    private SamplesManager sampleManager;
    private Dictionary<string, Sprite> sprites;
    private Dictionary<string, Dictionary<Dir, List<string>>> rules;
    private List<string> types;
    private List<TileData> data;

    private Tile2D[,] tiles2D;
    private Tile2D[,] newTiles2D;

    private List<Vector2Int> lowEntropyList;
    private List<(Vector2Int, string)> steps;
    private List<string> errorStates;


    // Use this for initialization
    void Start()
    {
        container = GetComponent<RectTransform>();
        sampleManager = new SamplesManager(path, xmlPath, tileSize, FilterMode.Point);

        tiles2D = new Tile2D[cols, rows];
        newTiles2D = new Tile2D[cols, rows];

        sprites = sampleManager.GetSprites();
        rules = sampleManager.GetRules();
        types = sampleManager.GetTypes();
        data = sampleManager.GetTileData();

        lowEntropyList = new List<Vector2Int>();

        steps = new List<(Vector2Int, string)>();
        errorStates = new List<string>();

        CreateGrid();
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

            //Vector2Int index = lowEntropyList[0];
            System.Random random = new();
            int index = random.Next(0, lowEntropyList.Count);
            Vector2Int cell = lowEntropyList[index];
            bool fertig = false;

            while (tiles2D[cell.x, cell.y].GetValidTypes().Count > 0 && !fertig)
            {
                tiles2D[cell.x, cell.y].Collapse();

                if (errorStates.Contains(CurrentState()))
                {
                    tiles2D[cell.x, cell.y].RemoveType(tiles2D[cell.x, cell.y].GetTileType());
                }

                else
                {
                    steps.Add(new(cell, tiles2D[cell.x, cell.y].GetTileType()));
                    fertig = true;
                }
            }

            if (!fertig)
            {
                tiles2D[cell.x, cell.y].ResetTile();
                errorStates.Add(CurrentState());

                if (steps.Count == 0)
                    Restart();

                else
                {
                    tiles2D[steps[^1].Item1.x, steps[^1].Item1.y].ResetTile();
                    steps.RemoveAt(steps.Count - 1);
                }
            }

            UpdateValids();

            foreach (var tile in tiles2D)
                if (tile.IsCollapsed() && tile.ObjectImageIsEmpty())
                    tile.SetObjectImage(sprites[tile.GetTileType()]);
        }
    }

    // Creates Grid adjusting cell size to container, adds a Tile to each cell.
    public void CreateGrid()
    {
        float cellSize = Mathf.Min(container.rect.width / cols, container.rect.height / rows);
        Vector2 offSet = new((-container.rect.width + cellSize) / 2, (container.rect.height - cellSize) / 2);
        Vector2 tileSize = new(cellSize, cellSize);

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector2 position = new Vector2(i, -j) * cellSize + offSet;
                tiles2D[i, j] = new Tile2D(container, position, tileSize, types, data, i, j);
            }
        }
    }

    // Returns a list of valid Tile types for a direction
    private List<string> GetValidsForDirection(string type, Dir dir)
    {
        return rules[type][dir];
    }

    // Updates lowEntropyList with the positions of all Tiles with the lowest entropy (number of valid types)
    private void UpdateEntropy()
    {
        int lowest = int.MaxValue;
        lowEntropyList.Clear();

        foreach (var tile in tiles2D)
        {
            if ((tile.IsCollapsed()) || (tile.GetValidTypes().Count > lowest))
                continue;

            if (tile.GetValidTypes().Count < lowest)
            {
                lowest = tile.GetValidTypes().Count;
                lowEntropyList.Clear();
                lowEntropyList.Add(tile.GetGridPosition());
            }

            else if (tile.GetValidTypes().Count == lowest)
                lowEntropyList.Add(tile.GetGridPosition());
        }
    }

    // Updates the validTypes for each Tile, by observing its adjacent Tiles
    private void UpdateValids()
    {
        Array.Copy(tiles2D, newTiles2D, tiles2D.Length);
        List<string> tmpValidTypes = new();

        foreach (var tile in tiles2D)
        {
            int x = tile.GetGridPosition().x;
            int y = tile.GetGridPosition().y;

            if (tile.IsCollapsed())
                newTiles2D[x, y] = tile;

            else
            {
                List<string> validTypes = types;

                if (x > 0)
                {
                    tmpValidTypes.Clear();
                    if (tiles2D[x - 1, y].IsCollapsed())
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles2D[x - 1, y].GetTileType(), Dir.Right));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                if (x < cols - 1)
                {
                    tmpValidTypes.Clear();
                    if (tiles2D[x + 1, y].IsCollapsed())
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles2D[x + 1, y].GetTileType(), Dir.Left));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                if (y > 0)
                {
                    tmpValidTypes.Clear();
                    if (tiles2D[x, y - 1].IsCollapsed())
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles2D[x, y - 1].GetTileType(), Dir.Down));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                if (y < rows - 1)
                {
                    tmpValidTypes.Clear();
                    if (tiles2D[x, y + 1].IsCollapsed())
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles2D[x, y + 1].GetTileType(), Dir.Up));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                newTiles2D[x, y].SetValidTypes(validTypes);
            }
        }

        tiles2D = newTiles2D;
    }

    // Returns true if all Tiles are collapsed
    private bool CheckFullyCollapsed()
    {
        foreach (var tile in tiles2D)
            if (!tile.IsCollapsed())
                return false;

        return true;
    }

    public void Restart()
    {
        foreach (var tile in tiles2D)
            tile.ResetTile();

        steps.Clear();
        lowEntropyList.Clear();
        errorStates.Clear();
    }

    private string CurrentState()
    {
        string state = "";
        string separator = "-";
        foreach (var tile in tiles2D)
        {
            if (!tile.IsCollapsed())
                state += "x" + separator;
            else
                state += types.IndexOf(tile.GetTileType()) + separator;
        }

        state = state.Remove(state.Length - 1);
        return state;
    }
}