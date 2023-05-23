using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

// Generates Sprites and adjacency rules from a sample
public class SampleManager2D
{
    public Dictionary<string, Sprite> sprites;
    public Dictionary<string, Dictionary<Dir, List<string>>> rules;

    private Dictionary<Dir, List<string>> adjacents;

    private readonly string samplePath;
    private readonly string xmlFilePath;
    private readonly int tileSize;
    private readonly FilterMode filterMode;
    private readonly Texture2D sourceTexture;

    public SampleManager2D(string samplePath, string xmlFilePath, int tileSize, FilterMode filterMode)
    {
        this.samplePath = samplePath;
        this.xmlFilePath = xmlFilePath;
        this.tileSize = tileSize;
        this.filterMode = filterMode;

        sprites = new Dictionary<string, Sprite>();
        rules = new Dictionary<string, Dictionary<Dir, List<string>>>();

        if (File.Exists(this.xmlFilePath))
        {
            sourceTexture = LoadSourceTexture();
            GenerateFromSource(false);
            LoadXML(this.xmlFilePath);
        }
        else
        {
            sourceTexture = LoadSourceTexture();
            GenerateFromSource(true);
            SaveToXML(this.xmlFilePath);
        }
    }

    private Texture2D LoadSourceTexture()
    {
        byte[] bytes = File.ReadAllBytes(samplePath);

        Texture2D sourceTexture = new(1, 1);
        sourceTexture.LoadImage(bytes);

        return sourceTexture;
    }

    private void GenerateFromSource(bool generateRules)
    {
        for (int i = 0; i < sourceTexture.width; i += tileSize)
        {
            for (int j = 0; j < sourceTexture.height; j += tileSize)
            {
                Texture2D blockTexture = new(tileSize, tileSize);
                Color[] pixelColors = sourceTexture.GetPixels(i, j, tileSize, tileSize);

                blockTexture.SetPixels(pixelColors);
                blockTexture.filterMode = filterMode;
                blockTexture.Apply();
                string blockHash = GetTextureHash(blockTexture);

                if (!sprites.ContainsKey(blockHash))
                {
                    Sprite sprite = Sprite.Create(blockTexture, new Rect(0.0f, 0.0f, blockTexture.width, blockTexture.height), new Vector2(0.5f, 0.5f));
                    sprites[blockHash] = sprite;

                    if (generateRules == true)
                    {
                        rules.Add(blockHash, GetValidAdjacencies(blockTexture));
                    }
                }
            }
        }
    }

    // TODO: Create function for repeated code
    private Dictionary<Dir, List<string>> GetValidAdjacencies(Texture2D texture)
    {
        adjacents = new()
        {
            { Dir.Up, new List<string>() },
            { Dir.Right, new List<string>() },
            { Dir.Down, new List<string>() },
            { Dir.Left, new List<string>() }
        };

        for (int i = 0; i < sourceTexture.width; i += tileSize)
        {
            for (int j = 0; j < sourceTexture.height; j += tileSize)
            {
                Texture2D blockTexture = new(tileSize, tileSize);
                blockTexture.SetPixels(sourceTexture.GetPixels(i, j, tileSize, tileSize));
                string blockHash = GetTextureHash(blockTexture);

                if (blockHash == GetTextureHash(texture))
                {
                    if (j < sourceTexture.height - tileSize) // North
                        AddAdjacent(i, j + tileSize, Dir.Up);

                    if (i < sourceTexture.width - tileSize) // East
                        AddAdjacent(i + tileSize, j, Dir.Right);

                    if (j > 0) // South
                        AddAdjacent(i, j - tileSize, Dir.Down);

                    if (i > 0) // West
                        AddAdjacent(i - tileSize, j, Dir.Left);
                }
            }
        }

        return adjacents;
    }

    private void AddAdjacent(int x, int y, Dir dir)
    {
        Texture2D adjacentTexture = new(tileSize, tileSize);
        adjacentTexture.SetPixels(sourceTexture.GetPixels(x, y, tileSize, tileSize));
        string adjacentHash = GetTextureHash(adjacentTexture);

        if (!adjacents[dir].Contains(adjacentHash))
            adjacents[dir].Add(adjacentHash);
    }

    private string GetTextureHash(Texture2D texture)
    {
        return texture.imageContentsHash.ToString();
    }

    private void SaveToXML(string xmlFilePath)
    {
        XmlDictionaryManager saver = new(xmlFilePath);
        saver.Save(rules);
    }

    private void LoadXML(string xmlFilePath)
    {
        XmlDictionaryManager loader = new(xmlFilePath);
        Dictionary<string, Dictionary<Dir, List<string>>> loadedRules = loader.Load();
        rules = loadedRules;
    }
}