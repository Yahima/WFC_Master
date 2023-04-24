using System;
using UnityEngine;

public class GridManager2D
{
    private RectTransform container;
    private readonly int width;
    private readonly int height;
    

    public GridManager2D(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public Vector2Int[,] CreateGrid()
    {
        Vector2Int[,] cells = new Vector2Int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                cells[x, y] = new Vector2Int(x, y);

            }
        }

        return cells;
    }
}
