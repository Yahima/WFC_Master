using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Tile2D
{
    public bool collapsed;
    public string type;
    public List<string> validTypes;
    public List<Tile> tiles;
    public Vector2Int gridPosition;
    public GameObject gameObject;

    public Tile2D(RectTransform container, Vector2 position, Vector2 size, List<string> types, List<Tile> tileTypes, int x, int y)
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
    }

    public void Collapse()
    {
        System.Random random = new();
        type = validTypes[random.Next(0, validTypes.Count)];
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

    public float GetEntropy()
    {
        float sumWeight = 0;
        float sumWeightLogWeight = 0;
        List<Tile> filteredTileList = tiles.Where(tile => validTypes.Contains(tile.name)).ToList();

        if (filteredTileList.Count == 0)
            return 0f;

        foreach (Tile tile in filteredTileList)
        {
            sumWeight += tile.weight;
            sumWeightLogWeight += tile.weight * Mathf.Log(tile.weight);
        }

        float entropy = Mathf.Log(sumWeight) - (sumWeightLogWeight / sumWeight);

        return entropy;
    }
}


