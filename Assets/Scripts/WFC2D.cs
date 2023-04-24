using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class WFC2D : MonoBehaviour
{
    public string path;
    public int tileSize;
    public int cols, rows;

    private RectTransform container;

    private Tile2D[,] tiles;
    private Tile2D[,] newTiles;

    private List<string> types;
    private Dictionary<string, Sprite> sprites;

    private List<Vector2Int> lowEntropyList;

    private List<(Vector2Int, string)> steps;
    SampleManager2D sampleManager;
    private Dictionary<string, Dictionary<Direction, List<string>>> rules;

    // Use this for initialization
    void Start()
    {
        sampleManager = new SampleManager2D(path, tileSize, FilterMode.Point);
        container = GetComponent<RectTransform>();

        tiles = new Tile2D[cols, rows];
        newTiles = new Tile2D[cols, rows];

        sprites = sampleManager.sprites;

        types = new List<string>();
        foreach (var kvp in sprites)
            types.Add(kvp.Key);

        lowEntropyList = new List<Vector2Int>();

        rules = sampleManager.rules;
        steps = new List<(Vector2Int, string)>();

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

            Vector2Int index = lowEntropyList[0];

            if (tiles[index.x, index.y].validTypes.Count > 0)
            {
                tiles[index.x, index.y].Collapse();
                steps.Add(new(index, tiles[index.x, index.y].type));
            }

            else
                while (!tiles[steps[^1].Item1.x, steps[^1].Item1.y].CollapseOther(steps[^1].Item2) && steps.Count > 0)
                    steps.RemoveAt(steps.Count - 1);


            UpdateValids();

            foreach (var tile in tiles)
                if (tile.collapsed && tile.gameObject.GetComponent<Image>().sprite == null)
                    tile.gameObject.GetComponent<Image>().sprite = sprites[tile.type];

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

                tiles[i, j] = new Tile2D(container, position, tileSize, types, i, j);
            }
        }
    }

    // Returns a list of valid Tile types for a direction
    private List<string> GetValidsForDirection(string type, Direction dir)
    {
        return rules[type][dir];
    }

    // Updates lowEntropyList with the positions of all Tiles with the lowest entropy (number of valid types)
    private void UpdateEntropy()
    {
        int lowest = int.MaxValue;
        lowEntropyList.Clear();

        foreach (var tile in tiles)
        {
            if ((tile.collapsed == true) || (tile.validTypes.Count > lowest))
                continue;

            if (tile.validTypes.Count < lowest)
            {
                lowest = tile.validTypes.Count();
                lowEntropyList.Clear();
                lowEntropyList.Add(tile.gridPosition);
            }

            else if (tile.validTypes.Count() == lowest)
                lowEntropyList.Add(tile.gridPosition);
        }
    }

    // Updates the validTypes for each Tile, by observing its adjacent Tiles
    private void UpdateValids()
    {
        Array.Copy(tiles, newTiles, tiles.Length);
        List<string> tmpValidTypes = new();

        foreach (var tile in tiles)
        {
            int x = tile.gridPosition.x;
            int y = tile.gridPosition.y;

            if (tile.collapsed)
                newTiles[x, y] = tile;

            else
            {
                List<string> validTypes = types;

                if (x > 0)
                {
                    tmpValidTypes.Clear();
                    if (tiles[x - 1, y].collapsed)
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles[x - 1, y].type, Direction.East));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                if (x < cols - 1)
                {
                    tmpValidTypes.Clear();
                    if (tiles[x + 1, y].collapsed)
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles[x + 1, y].type, Direction.West));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                newTiles[x, y].validTypes = validTypes;
                if (y > 0)
                {
                    tmpValidTypes.Clear();
                    if (tiles[x, y - 1].collapsed)
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles[x, y - 1].type, Direction.South));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                if (y < rows - 1)
                {
                    tmpValidTypes.Clear();
                    if (tiles[x, y + 1].collapsed)
                        tmpValidTypes.AddRange(GetValidsForDirection(tiles[x, y + 1].type, Direction.North));
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                newTiles[x, y].validTypes = validTypes;
            }
        }

        tiles = newTiles;
    }

    // Returns true if all Tiles are collapsed
    private bool CheckFullyCollapsed()
    {
        foreach (var tile in tiles)
            if (!tile.collapsed)
                return false;

        return true;
    }
}