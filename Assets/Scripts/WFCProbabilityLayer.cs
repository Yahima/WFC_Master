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
    public TMP_Dropdown dropdown;
    public int probabilityValue;

    private RectTransform container;

    private TextMeshProUGUI[,] valuesText;
    private int[,] values;

    private ProbabilityTile[,] probTiles;
    private ProbabilityTile[,] newProbTiles;

    private SamplesManager sampleManager;
    private Dictionary<string, Sprite> sprites;
    private Dictionary<string, Dictionary<Dir, List<string>>> rules;
    private List<string> types;
    private List<TileData> data;

    private List<Vector2Int> lowEntropyList;
    private List<string> errorStates;
    private List<History> history;

    private int valueDistributionMethod;
    private bool splatmap = false;
    private int[,] gridValues;

    void Start()
    {
        container = GetComponent<RectTransform>();

        valueToggle.onValueChanged.AddListener(delegate
        {
            ValueToggleValueChanged(valueToggle);
        });

        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(dropdown);
        });

        valuesText = new TextMeshProUGUI[cols, rows];
        values = new int[cols, rows];

        probTiles = new ProbabilityTile[cols, rows];
        newProbTiles = new ProbabilityTile[cols, rows];

        sampleManager = new SamplesManager(samplesPath, xmlFilePath, tileSize, FilterMode.Point);
        sprites = sampleManager.GetSprites();
        rules = sampleManager.GetRules();
        types = sampleManager.GetTypes();
        data = sampleManager.GetTileData();

        lowEntropyList = new List<Vector2Int>();

        errorStates = new List<string>();
        history = new List<History>();

        valueDistributionMethod = dropdown.value;
        gridValues = new int[cols, rows];

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
            int cellIndex = random.Next(0, lowEntropyList.Count);
            Vector2Int currentCell = lowEntropyList[cellIndex];

            bool fertig = false;

            while (probTiles[currentCell.x, currentCell.y].GetValidTypes().Count > 0 && !fertig)
            {
                probTiles[currentCell.x, currentCell.y].WeightedCollapse();

                if (errorStates.Contains(CurrentState()))
                {
                    probTiles[currentCell.x, currentCell.y].RemoveType(probTiles[currentCell.x, currentCell.y].GetTileType());
                }

                else
                {
                    history.Add(new History(CurrentState(), new(currentCell, probTiles[currentCell.x, currentCell.y].GetTileType())));
                    fertig = true;
                }
            }

            if (!fertig)
            {
                probTiles[currentCell.x, currentCell.y].ResetTile();
                errorStates.Add(CurrentState());

                List<Vector2Int> cellAdjacents = GetCellAdjacents(currentCell);
                History lastCollapsed = history
                    .Where(history => cellAdjacents.Contains(history.Step.Item1))
                    .LastOrDefault();

                int lastCollapsedIndex = history.IndexOf(lastCollapsed);
                LoadState(history[lastCollapsedIndex].CurrentState);
                errorStates.Add(CurrentState());
                foreach (ProbabilityTile tile in probTiles)
                {
                    if (cellAdjacents.Contains(tile.GetGridPosition()))
                    {
                        tile.ResetTile();
                    }
                }

                history.RemoveRange(lastCollapsedIndex, history.Count - lastCollapsedIndex);
            }

            UpdateValids();

            int count = 0;
            foreach (var tile in probTiles)
                if (tile.IsCollapsed())
                    count++;

            float percentage = ((float)count / (cols * rows)) * 100f;
            Debug.Log(percentage + "%");
        }

        else
        {
            foreach (var tile in probTiles)
                if (tile.IsCollapsed() && tile.ObjectImageIsEmpty())
                    tile.SetObjectImage(sprites[tile.GetTileType()]);

            if (!splatmap)
            {
                foreach (ProbabilityTile tile in probTiles)
                {
                    int value = data.FirstOrDefault(item => item.name == tile.GetTileType()).value;
                    gridValues[tile.GetGridPosition().x, tile.GetGridPosition().y] = value;
                }
                SplatmapGenerator splatmapGenerator = new SplatmapGenerator(40, gridValues);
                splatmap = true;
            }
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

    private void DropdownValueChanged(TMP_Dropdown dropdown)
    {
        valueDistributionMethod = dropdown.value;
        Restart();
    }

    // Creates Grid adjusting cell size to container, adds a Tile to each cell.
    public void CreateGrid()
    {
        float cellSize = Mathf.Min(container.rect.width / cols, container.rect.height / rows);
        Vector2 offSet = new((-container.rect.width + cellSize) / 2, (container.rect.height - cellSize) / 2);
        Vector2 tileSize = new(cellSize, cellSize);

        int cellValue = 0;
        int centerX = cols / 2;
        int centerY = rows / 2;
        int numRings = Math.Min(cols, rows) / 2;
        float scale = 0.1f;

        if (valueDistributionMethod == 0) // Growing Regions
            GrowingRegions();

        for (int i = 0; i < cols; i++)
        {
            if (valueDistributionMethod == 1)
                cellValue = Mathf.RoundToInt(Mathf.Lerp(maxValue, minValue, (float)i / (cols - 1)));

            for (int j = 0; j < rows; j++)
            {
                Vector2 position = new Vector2(i, -j) * cellSize + offSet;

                if (valueDistributionMethod == 1) // Horizontal
                    values[i, j] = (int)cellValue;

                if (valueDistributionMethod == 2) // Radial
                {
                    float distance = (float)Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                    float maxDistance = numRings * 1.0f;
                    float range = Math.Max(0.0f, (maxValue - minValue) * ((maxDistance - distance) / maxDistance));
                    cellValue = Mathf.RoundToInt(minValue + range);

                    values[i, j] = cellValue;
                }

                if (valueDistributionMethod == 3) // Perlin Noise
                {
                    float perlinValue = Mathf.PerlinNoise((i + UnityEngine.Random.value) * scale, (j + UnityEngine.Random.value) * scale);
                    float normalizedValue = perlinValue * (maxValue - minValue) + minValue; 
                    cellValue = Mathf.RoundToInt(normalizedValue); 

                    values[i, j] = cellValue;
                }

                probTiles[i, j] = new ProbabilityTile(container, position, tileSize, types, data, i, j, probabilityValue, values[i, j]);

                valuesText[i, j] = Instantiate(valueText, container);
                valuesText[i, j].rectTransform.anchoredPosition = position;
                valuesText[i, j].rectTransform.sizeDelta = new Vector2(cellSize, cellSize);

                valuesText[i, j].SetText(values[i, j].ToString());
            }
        }
    }

    private List<string> GetValidsForDirection(string type, Dir dir)
    {
        return rules[type][dir];
    }

    private List<Vector2Int> GetCellAdjacents(Vector2Int cell)
    {
        List<Vector2Int> adjacents = new();
        int x = cell.x;
        int y = cell.y;

        if (x > 0)
            if (probTiles[x - 1, y].IsCollapsed())
                adjacents.Add(probTiles[x - 1, y].GetGridPosition());

        if (x < cols - 1)
            if (probTiles[x + 1, y].IsCollapsed())
                adjacents.Add(probTiles[x + 1, y].GetGridPosition());

        if (y > 0)
            if (probTiles[x, y - 1].IsCollapsed())
                adjacents.Add(probTiles[x, y - 1].GetGridPosition());

        if (y < rows - 1)
            if (probTiles[x, y + 1].IsCollapsed())
                adjacents.Add(probTiles[x, y + 1].GetGridPosition());

        return adjacents;
    }

    private string CurrentState()
    {
        string state = "";
        string separator = "-";
        foreach (var tile in probTiles)
        {
            if (!tile.IsCollapsed())
                state += "x" + separator;
            else
                state += types.IndexOf(tile.GetTileType()).ToString() + separator;
        }

        state = state.Remove(state.Length - 1);
        return state;
    }

    private void LoadState(string state)
    {
        string[] tileStates = state.Split("-");
        int index = 0;
        foreach (var tile in probTiles)
        {
            if (tileStates[index].Equals("x"))
                tile.ResetTile();
            else
                tile.CollapseToType(types[int.Parse(tileStates[index])]);

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
            if ((tile.IsCollapsed() == true) || (tile.GetEntropy() > lowest))
                continue;

            if (tile.GetEntropy() < lowest)
            {
                lowest = tile.GetEntropy();
                lowEntropyList.Clear();
                lowEntropyList.Add(tile.GetGridPosition());
            }

            else if (tile.GetEntropy() == lowest)
                lowEntropyList.Add(tile.GetGridPosition());
        }
    }

    // Updates the validTypes for each Tile, by observing its adjacent Tiles
    private void UpdateValids()
    {
        Array.Copy(probTiles, newProbTiles, probTiles.Length);
        List<string> tmpValidTypes = new();

        foreach (var tile in probTiles)
        {
            int x = tile.GetGridPosition().x;
            int y = tile.GetGridPosition().y;

            if (tile.IsCollapsed())
                newProbTiles[x, y] = tile;

            else
            {
                List<string> validTypes = types;

                // look North
                if (y > 0)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x, y - 1].IsCollapsed())
                        tmpValidTypes = GetValidsForDirection(probTiles[x, y - 1].GetTileType(), Dir.Down);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                // look South
                if (y < rows - 1)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x, y + 1].IsCollapsed())
                        tmpValidTypes = GetValidsForDirection(probTiles[x, y + 1].GetTileType(), Dir.Up);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                // look West
                if (x > 0)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x - 1, y].IsCollapsed())
                        tmpValidTypes = GetValidsForDirection(probTiles[x - 1, y].GetTileType(), Dir.Right);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                // look East
                if (x < cols - 1)
                {
                    tmpValidTypes.Clear();
                    if (probTiles[x + 1, y].IsCollapsed())
                        tmpValidTypes = GetValidsForDirection(probTiles[x + 1, y].GetTileType(), Dir.Left);
                    else
                        tmpValidTypes = validTypes;

                    tmpValidTypes = tmpValidTypes.Distinct().ToList();
                    validTypes = validTypes.Intersect(tmpValidTypes).ToList();
                }

                newProbTiles[x, y].SetValidTypes(validTypes);
            }
        }

        probTiles = newProbTiles;
    }

    private bool CheckFullyCollapsed()
    {
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < rows; j++)
                if (probTiles[i, j].IsCollapsed() == false)
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

        //UnityEngine.Random.seed = 42;

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
            Destroy(tile.GetTileObject());

        foreach (TextMeshProUGUI tmp in valuesText)
            Destroy(tmp.gameObject);

        values = new int[cols, rows];
        probTiles = new ProbabilityTile[cols, rows];
        newProbTiles = new ProbabilityTile[cols, rows];

        history.Clear();
        lowEntropyList.Clear();
        errorStates.Clear();

        splatmap = false;

        CreateGrid();
        UpdateValids();
    }
}