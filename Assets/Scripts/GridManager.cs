using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    private readonly int width;
    private readonly int height;
    private readonly int depth;

    private readonly bool[,] visited;
    private readonly List<List<Vector2Int>> enclosedCellsGroups;

    public GridManager(int width, int height, int depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;

        visited = new bool[width, depth];
        enclosedCellsGroups = new List<List<Vector2Int>>();
    }

    public Vector3Int[,,] CreateGrid()
    {
        Vector3Int[,,] cells = new Vector3Int[width, height, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    cells[x, y, z] = new Vector3Int(x, y, z);
                }
            }
        }

        return cells;
    }

    public List<List<Vector2Int>> FindEnclosedCellGroups(int[,] grid)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j] == 0 && !visited[i, j])
                {
                    bool isEnclosed = true;
                    List<Vector2Int> enclosedCells = new List<Vector2Int>();

                    DFS(i, j, enclosedCells, grid, ref isEnclosed);
                    enclosedCellsGroups.Add(enclosedCells);
                }
            }
        }

        Debug.Log(enclosedCellsGroups.Count);
        foreach (var group in enclosedCellsGroups)
        {
            Debug.Log(enclosedCellsGroups.IndexOf(group) + ":" + group.Count);

            foreach (var cells in group)
            {
                Debug.Log(cells.ToString());
            }
        }

        return enclosedCellsGroups;
    }

    private void DFS(int i, int j, List<Vector2Int> enclosedCells, int[,] grid, ref bool isEnclosed)
    {
        if (i < 0 || i >= width || j < 0 || j >= depth)
        {
            isEnclosed = false;
            return;
        }
            

        if (visited[i, j])
            return;

        if (grid[i, j] == 1)
            return;

        visited[i, j] = true;
        enclosedCells.Add(new Vector2Int(i, j));

        DFS(i - 1, j, enclosedCells, grid, ref isEnclosed);
        DFS(i + 1, j, enclosedCells, grid, ref isEnclosed);
        DFS(i, j - 1, enclosedCells, grid, ref isEnclosed);
        DFS(i, j + 1, enclosedCells, grid, ref isEnclosed);
    }
}
