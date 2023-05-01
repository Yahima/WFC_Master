using UnityEngine;

public class GridManager
{
    private readonly int width;
    private readonly int height;
    private readonly int depth;

    public GridManager(int width, int height, int depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public Vector3Int[,,] CreateGrid()
    {
        Vector3Int[,,] cells = new Vector3Int[width, height, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y ++)
            {
                for (int z = 0; z < depth; z++)
                {
                    cells[x, y, z] = new Vector3Int(x, y, z);
                }
            }
        }

        return cells;
    }
}
