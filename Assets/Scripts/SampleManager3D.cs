using UnityEngine;
using System.Collections.Generic;
using System;

// Generates adjacency rules from sample modules
public class SampleManager3D
{
    private readonly GameObject[] sampleModules;
    private Dictionary<string, GameObject> gameObjects;
    private Dictionary<string, Dictionary<Dir, List<string>>> moduleRules;
    private int yOffset = 5;
    private int objectSize = 10;

    public SampleManager3D()
    {
        sampleModules = GameObject.FindGameObjectsWithTag("Module");
        gameObjects = new Dictionary<string, GameObject>();
    }

    public Dictionary<string, GameObject> GetObjects()
    {
        return gameObjects;
    }

    // Generates a list of objects and their adjacency rules
    public Dictionary<string, Dictionary<Dir, List<string>>> GenerateRulesFromSamples()
    {
        moduleRules = new Dictionary<string, Dictionary<Dir, List<string>>>();

        foreach (GameObject sampleModule in sampleModules)
        {
            if (!gameObjects.ContainsKey(sampleModule.name))
                gameObjects.Add(sampleModule.name, sampleModule);

            int rotations = GetRotations((int)sampleModule.transform.eulerAngles.y);
            string moduleName = sampleModule.name + rotations.ToString();

            if (!moduleRules.ContainsKey(moduleName))
            {
                Dictionary<Dir, List<string>> adjacents = GetAdjacents(moduleName);
                moduleRules.Add(moduleName, adjacents);

                string name = moduleName[0..^1];
                int rotation = int.Parse(moduleName[^1..]);

                for (int i = 0; i <= 3; i++)
                {
                    if (i != rotation)
                        moduleRules.Add(name + i.ToString(), RotateRules(adjacents, i, rotation));
                }
            }       
        }

        foreach (var rule in moduleRules)
        {
            Debug.Log("module:::::::::::::::::::::" + rule.Key);
            foreach (var dir in rule.Value)
            {
                Debug.Log(dir.Key);
                foreach (var name in dir.Value)
                {
                    Debug.Log(name);
                }
            }
        }

        Debug.Log(moduleRules.Count);

        return moduleRules;
    }

    private Dictionary<Dir, List<string>> RotateRules(Dictionary<Dir, List<string>> adjacents, int targetRotation, int currentRotation)
    {
        Dictionary<Dir, List<string>> rotatedAdjacents = new Dictionary<Dir, List<string>>();
        rotatedAdjacents.Add(Dir.Forward, new List<string>());
        rotatedAdjacents.Add(Dir.Back, new List<string>());
        rotatedAdjacents.Add(Dir.Left, new List<string>());
        rotatedAdjacents.Add(Dir.Right, new List<string>());
        rotatedAdjacents.Add(Dir.Up, adjacents[Dir.Up]);
        rotatedAdjacents.Add(Dir.Down, adjacents[Dir.Down]);

        int dirRotation = (targetRotation - currentRotation) % 4;
        if (dirRotation < 0)
            dirRotation += 4;

        switch (dirRotation)
        {
            case 1:

                foreach (string moduleName in adjacents[Dir.Right])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Forward].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Back])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Right].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Left])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Back].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Forward])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Left].Add(name + rotation.ToString());
                }

                break;

            case 2:

                foreach (string moduleName in adjacents[Dir.Back])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Forward].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Left])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Right].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Forward])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Back].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Right])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Left].Add(name + rotation.ToString());
                }

                break;

            case 3:

                foreach (string moduleName in adjacents[Dir.Left])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Forward].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Forward])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Right].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Right])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Back].Add(name + rotation.ToString());
                }

                foreach (string moduleName in adjacents[Dir.Back])
                {
                    string name = moduleName[0..^1];
                    int currentModuleRotation = int.Parse(moduleName[^1..]);

                    int rotation = (currentModuleRotation + dirRotation) % 4;
                    if (rotation < 0)
                        rotation += 4;

                    rotatedAdjacents[Dir.Left].Add(name + rotation.ToString());
                }

                break;
            default:
                break;
        }

        return rotatedAdjacents;
    }

    // Generates adjacency rules for each direction from sample
    private Dictionary<Dir, List<string>> GetAdjacents(string moduleName)
    {
        Dictionary<Dir, List<string>> adjacents = new Dictionary<Dir, List<string>>();
        adjacents.Add(Dir.Forward, new List<string>());
        adjacents.Add(Dir.Back, new List<string>());
        adjacents.Add(Dir.Left, new List<string>());
        adjacents.Add(Dir.Right, new List<string>());
        adjacents.Add(Dir.Up, new List<string>());
        adjacents.Add(Dir.Down, new List<string>());

        GameObject adjacentObject;

        foreach (GameObject sampleModule in sampleModules)
        {
            int rotations = GetRotations((int)sampleModule.transform.eulerAngles.y);
            string sampleModuleName = sampleModule.name + rotations.ToString();

            if (sampleModuleName == moduleName)
            {
                adjacentObject = FindObjectAtPosition(sampleModule.transform.position + (Vector3.forward * objectSize) + new Vector3(0, yOffset, 0));

                if (adjacentObject != null)
                {
                    string adjacentName = adjacentObject.name + GetRotations((int)adjacentObject.transform.eulerAngles.y);

                    if (!adjacents[Dir.Forward].Contains(adjacentName))
                        adjacents[Dir.Forward].Add(adjacentName);  
                }

                adjacentObject = FindObjectAtPosition(sampleModule.transform.position + (Vector3.back * objectSize) + new Vector3(0, yOffset, 0));

                if (adjacentObject != null)
                {
                    string adjacentName = adjacentObject.name + GetRotations((int)adjacentObject.transform.eulerAngles.y);

                    if (!adjacents[Dir.Back].Contains(adjacentName))
                        adjacents[Dir.Back].Add(adjacentName);
                }

                adjacentObject = FindObjectAtPosition(sampleModule.transform.position + (Vector3.left * objectSize) + new Vector3(0, yOffset, 0));

                if (adjacentObject != null)
                {
                    string adjacentName = adjacentObject.name + GetRotations((int)adjacentObject.transform.eulerAngles.y);

                    if (!adjacents[Dir.Left].Contains(adjacentName))
                        adjacents[Dir.Left].Add(adjacentName);
                }

                adjacentObject = FindObjectAtPosition(sampleModule.transform.position + (Vector3.right * objectSize) + new Vector3(0, yOffset, 0));

                if (adjacentObject != null)
                {
                    string adjacentName = adjacentObject.name + GetRotations((int)adjacentObject.transform.eulerAngles.y);

                    if (!adjacents[Dir.Right].Contains(adjacentName))
                        adjacents[Dir.Right].Add(adjacentName);
                }

                adjacentObject = FindObjectAtPosition(sampleModule.transform.position + (Vector3.up * objectSize) + new Vector3(0, yOffset, 0));

                if (adjacentObject != null)
                {
                    string adjacentName = adjacentObject.name + GetRotations((int)adjacentObject.transform.eulerAngles.y);

                    if (!adjacents[Dir.Up].Contains(adjacentName))
                        adjacents[Dir.Up].Add(adjacentName);
                }

                adjacentObject = FindObjectAtPosition(sampleModule.transform.position + (Vector3.down * objectSize) + new Vector3(0, yOffset, 0));

                if (adjacentObject != null)
                {
                    string adjacentName = adjacentObject.name + GetRotations((int)adjacentObject.transform.eulerAngles.y);

                    if (!adjacents[Dir.Down].Contains(adjacentName))
                        adjacents[Dir.Down].Add(adjacentName);
                }
            }
        }

        return adjacents;
    }

    // Gets the object at a given position
    private GameObject FindObjectAtPosition(Vector3 position)
    {
        GameObject gameObject = null;
        Collider[] colliders = Physics.OverlapSphere(position, 1f);

        if (colliders.Length > 0)
            gameObject = colliders[0].gameObject;

        return gameObject;
    }

    private int GetRotations(int angle)
    {
        int rotations = 0;

        switch (angle)
        {
            case 0:
                rotations = 0;
                break;
            case 90:
                rotations = 1;
                break;
            case 180:
                rotations = 2;
                break;
            case 270:
                rotations = 3;
                break;
            default:
                break;
        }
        return rotations;
    }
}
