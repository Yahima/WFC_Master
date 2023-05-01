using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class WFCProbabilityLayer : MonoBehaviour
{
    public string samplesPath, xmlFilePath;
    public int tileSize;
    public int cols, rows;
    public int minValue, maxValue;
    public TextMeshProUGUI valueText;
    public Toggle valueToggle;
    public int probabilityValue;

    private RectTransform container;

    private TextMeshProUGUI[,] valuesText;
    private int[,] values;

    private ProbabilityTile[,] probTiles;
    private ProbabilityTile[,] newProbTiles;

    private SamplesManager sampleManager;
    private Dictionary<string, Sprite> sprites;
    private Dictionary<string, Dictionary<Direction, List<string>>> rules;
    private List<string> types;
    private List<Tile> tiles;

    private List<Vector2Int> lowEntropyList;
    private List<(Vector2Int, string)> steps;

    // Use this for initialization
    void Start()
    {
        container = GetComponent<RectTransform>();

        valueToggle.onValueChanged.AddListener(delegate
        {
            ValueToggleValueChanged(valueToggle);
        });

        valuesText = new TextMeshProUGUI[cols, rows];
        values = new int[cols, rows];

        probTiles = new ProbabilityTile[cols, rows];
        newProbTiles = new ProbabilityTile[cols, rows];

        sampleManager = new SamplesManager(samplesPath, xmlFilePath, tileSize, FilterMode.Point);
        sprites = sampleManager.sprites;
        rules = sampleManager.rules;
        types = sampleManager.types;
        tiles = sampleManager.tiles;

        lowEntropyList = new List<Vector2Int>();
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

            System.Random random = new();
            int index = random.Next(0, lowEntropyList.Count);
            Vector2Int cell = lowEntropyList[index];

            if (probTiles[cell.x, cell.y].validTypes.Count > 0)
            {
                string value = Enum.GetName(typeof(Biome), values[cell.x, cell.y]);
                probTiles[cell.x, cell.y].WeightedCollapse(value);

                steps.Add(new(cell, probTiles[cell.x, cell.y].type));
            }

            else
                while (!probTiles[steps[^1].Item1.x, steps[^1].Item1.y].CollapseOther(steps[^1].Item2) && steps.Count > 0)
                    steps.RemoveAt(steps.Count - 1);


            UpdateValids();

            foreach (var tile in probTiles)
                if (tile.collapsed && tile.gameObject.GetComponent<Image>().sprite == null)
                    tile.gameObject.GetComponent<Image>().sprite = sprites[tile.type];
        }
    }

    private void ValueToggleValueChanged(Toggle toggle)
    {
        if (toggle.isOn)
            foreach (TextMeshProUGUI tmp in valuesText)
                tmp.gameObject.SetActive(true);

        else
            foreach (TextMeshProUGUI tmp in valuesText)
                tmp.gameObject.SetActive(false);
    }

    // Creates Grid adjusting cell size to container, adds a Tile to each cell.
    public void CreateGrid()
    {
        float cellSize = Mathf.Min(container.rect.width / cols, container.rect.height / rows);
        Vector2 offSet = new((-container.rect.width + cellSize) / 2, (container.rect.height - cellSize) / 2);
        Vector2 tileSize = new(cellSize, cellSize);

        GrowingRegions();

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector2 position = new Vector2(i, -j) * cellSize + offSet;

                probTiles[i, j] = new ProbabilityTile(container, position, tileSize, types, tiles, i, j, probabilityValue);

                valuesText[i, j] = Instantiate(valueText, container);
                valuesText[i, j].rectTransform.anchoredPosition = position;
                valuesText[i, j].rectTransform.sizeDelta = new Vector2(cellSize, cellSize);

                valuesText[i, j].SetText(values[i, j].ToString());
            }
        }

        for (int m = 1; m <= maxValue; m++)
        {
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (values[i, j] == m)
                    {
                        probTiles[i, j].CollapseTo(Enum.GetName(typeof(Biome), m));
                        goto LoopEnd;
                    }
                }
            }
        LoopEnd:;
        }
    }

    private List<string> GetValidsForDirection(string type, Direction dir)
    {
        return rules[type][dir];
    }

    // Updates lowEntropyList with the positions of all Tiles with the lowest entropy (number of valid types)
    private void UpdateEntropy()
    {
        int lowest = int.MaxValue;
        lowEntropyList.Clear();

        foreach (var tile in probTiles)
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
        Array.Copy(probTiles, newProbTiles, probTiles.Length);
        List<string> tmpValidTypes = new();

        foreach (var tile in probTiles)
        {
            int x = tile.gridPosition.x;
            int y = tile.gridPosition.y;

            if (tile.collapsed)
                newProbTiles[x, y] = tile;

            else
            {
                List<string> validTypes = types;

                // look North
                if (y > 0)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x, y - 1].collapsed)
                        tmpValidTypes = GetValidsForDirection(probTiles[x, y - 1].type, Direction.South);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                // look South
                if (y < rows - 1)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x, y + 1].collapsed)
                        tmpValidTypes = GetValidsForDirection(probTiles[x, y + 1].type, Direction.North);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                // look West
                if (x > 0)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x - 1, y].collapsed)
                        tmpValidTypes = GetValidsForDirection(probTiles[x - 1, y].type, Direction.East);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                // look East
                if (x < cols - 1)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x + 1, y].collapsed)
                        tmpValidTypes = GetValidsForDirection(probTiles[x + 1, y].type, Direction.West);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                newProbTiles[x, y].validTypes = validTypes;
            }
        }

        probTiles = newProbTiles;
    }

    private bool CheckFullyCollapsed()
    {
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < rows; j++)
                if (probTiles[i, j].collapsed == false)
                    return false;
        return true;
    }

    private void GrowingRegions()
    {
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                values[i, j] = 0;
            }
        }

        int regionValue = 1;
        int cellsPerRegion = cols * rows / maxValue;

        while (regionValue <= maxValue)
        {
            int startRow, startCol;
            do
            {
                startRow = UnityEngine.Random.Range(0, cols);
                startCol = UnityEngine.Random.Range(0, rows);
            } while (values[startRow, startCol] != 0);

            List<int[]> regionCells = new();
            regionCells.Add(new int[] { startRow, startCol });
            values[startRow, startCol] = regionValue;

            while (regionCells.Count < cellsPerRegion)
            {
                int[] nextCell = null;

                foreach (int[] cell in regionCells)
                {
                    int row = cell[0];
                    int col = cell[1];

                    int[][] neighbors = new int[][] {
                        new int[] {row - 1, col},
                        new int[] {row + 1, col},
                        new int[] {row, col - 1},
                        new int[] {row, col + 1}
                    };

                    foreach (int[] neighbor in neighbors)
                    {
                        int neighborRow = neighbor[0];
                        int neighborCol = neighbor[1];
                        if (neighborRow >= 0 && neighborRow < cols &&
                            neighborCol >= 0 && neighborCol < rows &&
                            values[neighborRow, neighborCol] == 0)
                        {
                            nextCell = neighbor;
                            break;
                        }
                    }

                    if (nextCell != null)
                        break;
                }

                if (nextCell == null)
                    break;

                regionCells.Add(nextCell);
                int nextRow = nextCell[0];
                int nextCol = nextCell[1];
                values[nextRow, nextCol] = regionValue;
            }

            regionValue++;
        }
    }

    public void Restart()
    {
        foreach (ProbabilityTile tile in probTiles)
            Destroy(tile.gameObject);

        foreach (TextMeshProUGUI tmp in valuesText)
            Destroy(tmp.gameObject);

        values = new int[cols, rows];
        probTiles = new ProbabilityTile[cols, rows];
        newProbTiles = new ProbabilityTile[cols, rows];

        steps.Clear();
        lowEntropyList.Clear();

        CreateGrid();
        UpdateValids();
    }
}