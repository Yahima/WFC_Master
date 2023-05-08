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
    public string set;

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
    private List<(string, (Vector2Int, string))> history;

    List<string> errorStates;
    List<string> states;
    
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
        history = new List<(string, (Vector2Int, string))>();

        states = new List<string>();
        errorStates = new List<string>();

        CreateGrid();
        UpdateValids();
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckFullyCollapsed() == false)
        {
            states.Add(CurrentState());
            UpdateEntropy();



            if (lowEntropyList.Count <= 0)
            {
                return;
            }

            System.Random random = new();
            int index = random.Next(0, lowEntropyList.Count);
            Vector2Int cell = lowEntropyList[0];
            bool fertig = false;

            while (probTiles[cell.x, cell.y].validTypes.Count > 0 && !fertig)
            {
                probTiles[cell.x, cell.y].WeightedCollapse();
                if (errorStates.Contains(CurrentState()))// NextState(cell, probTiles[cell.x, cell.y].type)))
                {
                    probTiles[cell.x, cell.y].validTypes.Remove(probTiles[cell.x, cell.y].type);
                } else
                {
                    steps.Add(new(cell, probTiles[cell.x, cell.y].type));
                    fertig = true;
                }
            }

            if (!fertig)
            {
                //while (!probTiles[steps[^1].Item1.x, steps[^1].Item1.y].CollapseOther(steps[^1].Item2) && steps.Count > 0)
                probTiles[cell.x, cell.y].ResetTile();
                errorStates.Add(CurrentState());
                steps.RemoveAt(steps.Count - 1);
                probTiles[steps[^1].Item1.x, steps[^1].Item1.y].ResetTile();
            }


            UpdateValids();
            
            foreach (var tile in probTiles)
                if (tile.collapsed && tile.gameObject.GetComponent<Image>().sprite == null)
                    tile.gameObject.GetComponent<Image>().sprite = sprites[tile.type];

        }

        else
        {
            
        }

        int oneCount = 0;

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (probTiles[i, j].collapsed)
                {
                    oneCount++;
                }
            }
        }

        float percentage = ((float)oneCount / (cols*rows)) * 100f;
        Debug.Log(percentage);
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

                probTiles[i, j] = new ProbabilityTile(container, position, tileSize, types, tiles, i, j, probabilityValue, values[i, j]);

                valuesText[i, j] = Instantiate(valueText, container);
                valuesText[i, j].rectTransform.anchoredPosition = position;
                valuesText[i, j].rectTransform.sizeDelta = new Vector2(cellSize, cellSize);

                valuesText[i, j].SetText(values[i, j].ToString());

            }
        }
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                valuesText[i, j].text = values[i, j].ToString();
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
                        probTiles[i, j].CollapseToValue();
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

    private string CurrentState()
    {
        string state = "";
        string separator = "-";
        foreach (var tile in probTiles)
        {
            if (!tile.collapsed)
            {
                state += "x" + separator;
            }
            else
            {
                state += types.IndexOf(tile.type).ToString() + separator;
            }
        }
        state = state.Remove(state.Length - 1);
        return state;
    }

    private string NextState(Vector2Int cell, string type)
    {
        string state = "";
        string separator = "-";
        foreach (var tile in probTiles)
        {
            if (tile.gridPosition.x == cell.x && tile.gridPosition.y == cell.y)
            {
                state += types.IndexOf(type).ToString() + separator;
            }
            else if (!tile.collapsed)
            {
                state += "x" + separator;
            }
            else
            {
                state += types.IndexOf(tile.type).ToString() + separator;
            }
        }
        state = state.Remove(state.Length - 1);
        //Debug.Log(state);
        return state;
    }

    private void LoadState(string state)
    {
        string[] tileStates = state.Split("-");
        int index = 0;
        foreach (var tile in probTiles)
        {
            if(tileStates[index].Equals("x"))
            {
                tile.ResetTile();
            }
            else
            {
                tile.CollapseToType(types[int.Parse(tileStates[index])]);
            }
            index++;
        }
    }

    // Updates lowEntropyList with the positions of all Tiles with the lowest entropy
    private void UpdateEntropy()
    {
        float lowest = float.MaxValue;
        lowEntropyList.Clear();

        foreach (var tile in probTiles)
        {
            if ((tile.collapsed == true) || (tile.GetEntropy() > lowest))
                continue;

            if (tile.GetEntropy() < lowest)
            {
                lowest = tile.GetEntropy();
                lowEntropyList.Clear();
                lowEntropyList.Add(tile.gridPosition);
            }

            else if (tile.GetEntropy() == lowest)
                lowEntropyList.Add(tile.gridPosition);
        }
        //if (lowest == 0)
        //{
        //    errorStates.Add(CurrentState());

        //    return;
        //}
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
        history.Clear();
        errorStates.Clear();

        CreateGrid();
        UpdateValids();
    }
}