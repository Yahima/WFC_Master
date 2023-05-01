using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile2D
{
    public bool collapsed;
    public string type;
    public List<string> validTypes;
    public Vector2Int gridPosition;
    public GameObject gameObject;

    public Tile2D(RectTransform container, Vector2 position, Vector2 size, List<string> types, int x, int y)
    {
        collapsed = false;
        validTypes = types;
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
}


