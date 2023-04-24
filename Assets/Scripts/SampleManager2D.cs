using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

// Generates Sprites and adjacency rules from a sample
public class SampleManager2D
{
    public Dictionary<string, Sprite> sprites;
    public Dictionary<string, Dictionary<Direction, List<string>>> rules;

    private readonly string path;
    private readonly int tileSize;
    private readonly FilterMode filterMode;
    private readonly Texture2D sourceTexture;

    public SampleManager2D(string path, int tileSize, FilterMode filterMode)
    {
        this.path = path;
        this.tileSize = tileSize;
        this.filterMode = filterMode;

        sprites = new Dictionary<string, Sprite>();
        rules = new Dictionary<string, Dictionary<Direction, List<string>>>();
        sourceTexture = LoadSourceTexture();
        GenerateFromSource();
    }

    private Texture2D LoadSourceTexture()
    {
        byte[] bytes = File.ReadAllBytes(path);

        Texture2D sourceTexture = new(1, 1);
        sourceTexture.LoadImage(bytes);

        return sourceTexture;
    }

    private void GenerateFromSource()
    {
        for (int i = 0; i < sourceTexture.width; i += tileSize)
        {
            for (int j = 0; j < sourceTexture.height; j += tileSize)
            {
                Texture2D blockTexture = new(tileSize, tileSize);
                Color[] pixelColors = sourceTexture.GetPixels(i, j, tileSize, tileSize);
                List<Color> tileColors = pixelColors.Distinct().ToList();

                blockTexture.SetPixels(pixelColors);
                blockTexture.filterMode = filterMode;
                blockTexture.Apply();
                string blockHash = GetTextureHash(blockTexture);

                if (!sprites.ContainsKey(blockHash))
                {
                    Sprite sprite = Sprite.Create(blockTexture, new Rect(0.0f, 0.0f, blockTexture.width, blockTexture.height), new Vector2(0.5f, 0.5f));
                    sprites[blockHash] = sprite;
                    rules.Add(blockHash, GetValidAdjacencies(blockTexture));
                }
            }
        }
    }

    // TODO: Create function for repeated code
    private Dictionary<Direction, List<string>> GetValidAdjacencies(Texture2D texture)
    {
        Dictionary<Direction, List<string>> adjacents = new()
        {
            { Direction.North, new List<string>() },
            { Direction.East, new List<string>() },
            { Direction.South, new List<string>() },
            { Direction.West, new List<string>() }
        };

        for (int i = 0; i < sourceTexture.width; i += tileSize)
        {
            for (int j = 0; j < sourceTexture.height; j += tileSize)
            {
                Texture2D blockTexture = new(tileSize, tileSize);
                Texture2D adjacentTexture = new(tileSize, tileSize);
                blockTexture.SetPixels(sourceTexture.GetPixels(i, j, tileSize, tileSize));
                string blockHash = GetTextureHash(blockTexture);

                if (blockHash == GetTextureHash(texture))
                {

                    if (j < sourceTexture.height - tileSize) // North
                    {
                        adjacentTexture = new(tileSize, tileSize);
                        adjacentTexture.SetPixels(sourceTexture.GetPixels(i, j + tileSize, tileSize, tileSize));
                        string adjacentHash = GetTextureHash(adjacentTexture);

                        if (!adjacents[Direction.North].Contains(adjacentHash))
                            adjacents[Direction.North].Add(adjacentHash);
                    }

                    if (i < sourceTexture.width - tileSize) // East
                    {
                        adjacentTexture = new(tileSize, tileSize);
                        adjacentTexture.SetPixels(sourceTexture.GetPixels(i + tileSize, j, tileSize, tileSize));
                        string adjacentHash = GetTextureHash(adjacentTexture);

                        if (!adjacents[Direction.East].Contains(adjacentHash))
                            adjacents[Direction.East].Add(adjacentHash);
                    }

                    if (j > 0) // South
                    {
                        adjacentTexture = new(tileSize, tileSize);
                        adjacentTexture.SetPixels(sourceTexture.GetPixels(i, j - tileSize, tileSize, tileSize));
                        string adjacentHash = GetTextureHash(adjacentTexture);

                        if (!adjacents[Direction.South].Contains(adjacentHash))
                            adjacents[Direction.South].Add(adjacentHash);
                    }

                    if (i > 0) // West
                    {
                        adjacentTexture = new(tileSize, tileSize);
                        adjacentTexture.SetPixels(sourceTexture.GetPixels(i - tileSize, j, tileSize, tileSize));
                        string adjacentHash = GetTextureHash(adjacentTexture);

                        if (!adjacents[Direction.West].Contains(adjacentHash))
                            adjacents[Direction.West].Add(adjacentHash);
                    }
                }
            }
        }

        return adjacents;
    }

    // TODO: Texture.imageContentsHash
    private string GetTextureHash(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        string hash = System.Convert.ToBase64String(bytes);
        return hash;
    }
}

