using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Tile2D
{
    private bool collapsed;
    private string type;
    private List<string> validTypes;
    private readonly List<TileData> tileData;
    private Vector2Int gridPosition;
    private readonly GameObject tileObject;

    public Tile2D(RectTransform container, Vector2 position, Vector2 size, List<string> types, List<TileData> data, int x, int y)
    {
        collapsed = false;
        validTypes = types;
        tileData = data;
        gridPosition = new Vector2Int(x, y);

        tileObject = new GameObject(x + ":" + y);

        RectTransform trans = tileObject.AddComponent<RectTransform>();
        trans.transform.SetParent(container);
        trans.localScale = Vector3.one;
        trans.anchoredPosition = position;
        trans.sizeDelta = size;

        tileObject.AddComponent<Image>();
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
        tileObject.GetComponent<Image>().sprite = null;
    }

    public float GetEntropy()
    {
        float sumWeight = 0;
        float sumWeightLogWeight = 0;
        List<TileData> filteredTileList = tileData.Where(tile => validTypes.Contains(tile.name)).ToList();

        if (filteredTileList.Count == 0)
            return 0f;

        foreach (TileData tile in filteredTileList)
        {
            sumWeight += tile.weight;
            sumWeightLogWeight += tile.weight * Mathf.Log(tile.weight);
        }

        float entropy = Mathf.Log(sumWeight) - (sumWeightLogWeight / sumWeight);

        return entropy;
    }

    public bool IsCollapsed()
    {
        return collapsed;
    }

    public string GetTileType()
    {
        return type;
    }

    public List<string> GetValidTypes()
    {
        return validTypes;
    }

    public void SetValidTypes(List<string> valids)
    {
        validTypes = valids;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    public GameObject GetTileObject()
    {
        return tileObject;
    }

    public void SetObjectImage(Sprite sprite)
    {
        tileObject.GetComponent<Image>().sprite = sprite;
    }

    public bool ObjectImageIsEmpty()
    {
        return tileObject.GetComponent<Image>().sprite == null;
    }

    public void RemoveType(string type)
    {
        validTypes.Remove(type);
    }
}


