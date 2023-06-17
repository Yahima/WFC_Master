using UnityEngine;

public class SplatmapGenerator
{
    public Color[] colors; // Array of colors corresponding to different values

    private int[,] values; // Input array
    private int squareSize = 10; // Size of each square in pixels
    private Texture2D splatmapTexture;

    public SplatmapGenerator(int squareSize, int[,] values)
    {
        this.squareSize = squareSize;
        this.values = values;
        colors = new Color[] {Color.red, Color.green, Color.blue, Color.black};
        GenerateSplatmapTexture();

    }

    // Call this method to generate the splatmap texture
    public void GenerateSplatmapTexture()
    {
        int width = values.GetLength(0) * squareSize;
        int height = values.GetLength(1) * squareSize;

        splatmapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        splatmapTexture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(1); y++)
            {
                Color color = GetColorForValue(values[x, y]);
                FillSquare(x, y, color);
            }
        }

        // Apply blur to the texture using a blur algorithm of your choice

        ExportTextureToFile();
    }

    private Color GetColorForValue(int value)
    {
        return colors[value - 1];
    }

    private void FillSquare(int x, int y, Color color)
    {
        for (int i = 0; i < squareSize; i++)
        {
            for (int j = 0; j < squareSize; j++)
            {
                int pixelX = x * squareSize + i;
                int pixelY = y * squareSize + j;
                splatmapTexture.SetPixel(pixelX, pixelY, color);
            }
        }
    }

    private void ExportTextureToFile()
    {
        byte[] bytes = splatmapTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes("Splatmap.png", bytes);
    }
}
