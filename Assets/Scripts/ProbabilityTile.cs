using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProbabilityTile
{
    public bool collapsed;
    public string type;
    public List<string> validTypes;
    public List<Tile> tiles;
    public Vector2Int gridPosition;
    public GameObject gameObject;
    public bool visited = false;

    
    private readonly int probability;
    private readonly int cellValue;

    public ProbabilityTile(RectTransform container, Vector2 position, Vector2 size, List<string> types, List<Tile> tileTypes, int x, int y, int probability, int cellValue)
    {
        collapsed = false;
        validTypes = types;
        tiles = tileTypes;
        gridPosition = new Vector2Int(x, y);

        gameObject = new GameObject(x + ":" + y);

        RectTransform trans = gameObject.AddComponent<RectTransform>();
        trans.transform.SetParent(container);
        trans.localScale = Vector3.one;
        trans.anchoredPosition = position;
        trans.sizeDelta = size;

        gameObject.AddComponent<Image>();

        this.probability = probability;
        this.cellValue = cellValue;
    }

    public void Collapse()
    {
        System.Random random = new();
        type = validTypes[random.Next(0, validTypes.Count)];
        collapsed = true;
    }

    public void CollapseToValue()
    {
        foreach (Tile tile in tiles)
        {
            if (tile.value == cellValue)
            {
                type = tile.name;
                collapsed = true;
                break;
            }
        }
    }

    public void CollapseToType(string type)
    {
        ResetTile();
        this.type = type;
        collapsed = true;
    }

    public void WeightedCollapse()
    {
        List<Tile> filteredTileList = tiles.Where(tile => validTypes.Contains(tile.name)).ToList();
        type = GetTypeByProbability(filteredTileList);
        collapsed = true;
        visited = true;
    }

    public bool CollapseOther(string type)
    {
        ResetTile();

        if (validTypes.Count > 1)
        {
            List<string> newTypes = validTypes;
            newTypes.Remove(type);

            System.Random random = new();
            this.type = newTypes[random.Next(0, newTypes.Count)];
            collapsed = true;
            
        }
        Debug.Log(collapsed);
        return collapsed;
    }

    public void ResetTile()
    {
        collapsed = false;
        gameObject.GetComponent<Image>().sprite = null;
        visited = false;
    }

    public string GetTypeByProbability(List<Tile> tiles)
    {
        List<string> weightedTypeList = new();

        foreach (var tile in tiles)
            if (tile.value == cellValue)
                for (int i = 0; i < probability; i++)
                    weightedTypeList.Add(tile.name);

            else
                weightedTypeList.Add(tile.name);

        System.Random random = new();
        return weightedTypeList[random.Next(0, weightedTypeList.Count)];
    }

    public void RemoveType(string type)
    {
        validTypes.Remove(type);
    }

    public float GetEntropy()
    {
        float sumWeight = 0;
        float sumWeightLogWeight = 0;
        List<Tile> filteredTileList = tiles.Where(tile => validTypes.Contains(tile.name)).ToList();

        if (filteredTileList.Count == 0)
        {
            return 0f; // Return 0 entropy for empty list
        }

        foreach (Tile tile in filteredTileList)
        {
            if (tile.value == cellValue)
            {
                tile.weight += probability;
            }
            sumWeight += tile.weight;
            sumWeightLogWeight += tile.weight * Mathf.Log(tile.weight);
        }

        float entropy = Mathf.Log(sumWeight) - (sumWeightLogWeight / sumWeight);

        return entropy;
    }
}

