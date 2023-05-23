using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridBlockManager
{
    private int gridSize;

    public int[,] cells;

    public GridBlockManager(int gridSize)
    {
        this.gridSize = gridSize;
    }

    public List<List<Vector2Int>> DivideGridIntoBlocks(int blockSize)
    {
        List<List<Vector2Int>> blocks = new List<List<Vector2Int>>();

        for (int i = 0; i < gridSize; i += blockSize)
        {
            for (int j = 0; j < gridSize; j += blockSize)
            {
                List<Vector2Int> blockCells = new List<Vector2Int>();

                for (int x = i; x < i + blockSize; x++)
                {
                    for (int y = j; y < j + blockSize; y++)
                    {
                        blockCells.Add(new Vector2Int(x, y));
                    }
                }

                blocks.Add(blockCells);
            }
        }

        return blocks;
    }

    public List<Vector2Int> GetAdjacentCells(List<Vector2Int> block, int size) // try making adjacentcells bigger
    {
        List<Vector2Int> adjacentCells = new List<Vector2Int>();

        for (int i = 1; i <= size; i++)
        {
            foreach (Vector2Int cell in block)
            {
                if (cell.x > 0)
                {
                    adjacentCells.Add(new Vector2Int(cell.x - i, cell.y));
                }
                if (cell.x < gridSize - i)
                {
                    adjacentCells.Add(new Vector2Int(cell.x + i, cell.y));
                }
                if (cell.y > 0)
                {
                    adjacentCells.Add(new Vector2Int(cell.x, cell.y - i));
                }
                if (cell.y < gridSize - i)
                {
                    adjacentCells.Add(new Vector2Int(cell.x, cell.y + i));
                }
            }
        }

        adjacentCells = adjacentCells.Distinct().ToList();
        return adjacentCells;
    }
}