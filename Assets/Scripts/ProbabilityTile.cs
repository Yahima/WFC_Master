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

    private readonly int probability;

    public ProbabilityTile(RectTransform container, Vector2 position, Vector2 size, List<string> types, List<Tile> tileTypes, int x, int y, int probability)
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
    }

    public void Collapse()
    {
        System.Random random = new();
        type = validTypes[random.Next(0, validTypes.Count)];
        collapsed = true;
    }

    public void CollapseTo(string value)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.type == value)
            {
                type = tile.name;
                collapsed = true;
                break;
            }
        }
    }

    public void WeightedCollapse(string value)
    {
        List<Tile> filteredTileList = tiles.Where(tile => validTypes.Contains(tile.name)).ToList();
        type = GetTypeByProbability(filteredTileList, value);
        collapsed = true;
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

        return collapsed;
    }

    public void ResetTile()
    {
        collapsed = false;
        gameObject.GetComponent<Image>().sprite = null;
    }

    public string GetTypeByProbability(List<Tile> validTypes, string value)
    {
        List<string> weightedTypeList = new();

        foreach (var valid in validTypes)
            if (valid.type == value)
                for (int i = 0; i < probability; i++)
                    weightedTypeList.Add(valid.name);
            else
                weightedTypeList.Add(valid.name);

        System.Random random = new();
        return weightedTypeList[random.Next(0, weightedTypeList.Count)];
    }
}

