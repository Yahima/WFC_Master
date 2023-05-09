using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Generates Sprites and adjacency rules from multiple samples
public class SamplesManager
{
    public Dictionary<string, Sprite> sprites;
    public Dictionary<string, Dictionary<Direction, List<string>>> rules;
    public List<string> types;
    public List<Tile> tiles;

    private readonly string samplesPath;
    private readonly string xmlFilePath;
    private readonly int tileSize;
    private readonly FilterMode filterMode;

    private string tileHash;
    private string adjacentTileHash;

    public SamplesManager(string samplesPath, string xmlFilePath, int tileSize, FilterMode filterMode)
    {
        this.samplesPath = samplesPath;
        this.xmlFilePath = xmlFilePath;
        this.tileSize = tileSize;
        this.filterMode = filterMode;

        sprites = new Dictionary<string, Sprite>();
        rules = new Dictionary<string, Dictionary<Direction, List<string>>>();
        types = new List<string>();
        tiles = new List<Tile>();

        ExtractTilesFromSamples();
    }

    private void ExtractTilesFromSamples()
    {
        foreach (Object obj in Resources.LoadAll(samplesPath, typeof(Texture2D)))
        {
            Texture2D sampleTexture = (Texture2D)obj;

            for (int i = 0; i < sampleTexture.width; i += tileSize)
            {
                for (int j = 0; j < sampleTexture.height; j += tileSize)
                {
                    Texture2D tileTexture = new(tileSize, tileSize);
                    tileTexture.SetPixels(sampleTexture.GetPixels(i, j, tileSize, tileSize));
                    tileTexture.filterMode = filterMode;
                    tileTexture.Apply();
                    tileHash = GenerateTextureHash(tileTexture);

                    if (!rules.Keys.Contains(tileHash))
                    {
                        Sprite sprite = Sprite.Create(tileTexture, new Rect(0.0f, 0.0f, tileTexture.width, tileTexture.height), new Vector2(0.5f, 0.5f));
                        sprites[tileHash] = sprite;

                        types.Add(tileHash);

                        string filename = Path.GetFileNameWithoutExtension(obj.name);
                        int value = int.Parse(filename.Substring(0, 3));
                        int weight = 1;

                        Tile tile = new(tileHash, value, weight);
                        tiles.Add(tile);

                        Dictionary<Direction, List<string>> validsForDirection = new()
                        {
                            { Direction.North, new List<string>() },
                            { Direction.East, new List<string>() },
                            { Direction.South, new List<string>() },
                            { Direction.West, new List<string>() }
                        };

                        rules.Add(tileHash, validsForDirection);
                        GetAdjacentTilesFromSample(sampleTexture, tileHash);
                    }

                    else
                    {
                        Tile existingTile = tiles.FirstOrDefault(tile => tile.name == tileHash);
                        existingTile.weight += 1;

                        GetAdjacentTilesFromSample(sampleTexture, tileHash);
                    }
                }
            }
        }
        foreach (var tile in tiles)
        {
            Debug.Log(tile.value + " : " + tile.weight);
        }
        Debug.Log(rules.Count);
    }

    private void GetAdjacentTilesFromSample(Texture2D sampleTexture, string tileHash)
    {
        for (int i = 0; i < sampleTexture.width; i += tileSize)
        {
            for (int j = 0; j < sampleTexture.height; j += tileSize)
            {
                Texture2D tileTexture = new(tileSize, tileSize);
                tileTexture.SetPixels(sampleTexture.GetPixels(i, j, tileSize, tileSize));
                string tileTextureHash = GenerateTextureHash(tileTexture);

                if (tileTextureHash == tileHash)
                {
                    if (j < sampleTexture.height - tileSize)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i, j + tileSize, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Direction.North, tileHash, adjacentTileHash);
                    }

                    if (i < sampleTexture.width - tileSize)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i + tileSize, j, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Direction.East, tileHash, adjacentTileHash);
                    }

                    if (j > 0)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i, j - tileSize, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Direction.South, tileHash, adjacentTileHash);
                    }

                    if (i > 0)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i - tileSize, j, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Direction.West, tileHash, adjacentTileHash);
                    }
                }
            }
        }
    }

    private void AddAdjacent(Direction dir, string tileHash, string adjacentTileHash)
    {
        if (!rules[tileHash][dir].Contains(adjacentTileHash))
            rules[tileHash][dir].Add(adjacentTileHash);
    }

    private string GenerateTextureHash(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        string hash = System.Convert.ToBase64String(bytes);
        return hash;
    }

    private void SaveToXML(string xmlFilePath)
    {
        XmlDictionaryManager saver = new(xmlFilePath);
        saver.Save(rules);
    }

    private void LoadXML(string xmlFilePath)
    {
        XmlDictionaryManager loader = new(xmlFilePath);
        Dictionary<string, Dictionary<Direction, List<string>>> loadedRules = loader.Load();
        rules = loadedRules;
    }
}