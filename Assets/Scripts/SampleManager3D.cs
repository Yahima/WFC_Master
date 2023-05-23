using UnityEngine;
using System.Collections.Generic;

// Generates adjacency rules from sample modules
public class SampleManager3D
{
    private GameObject[] sampleModules;

    public SampleManager3D()
    {
        sampleModules = GameObject.FindGameObjectsWithTag("Module");
    }

    // Creates a list of objects and corresponding name
    public Dictionary<string, GameObject> GetObjects()
    {
        Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
        foreach (var module in sampleModules)
            if (!gameObjects.ContainsKey(module.name))
                gameObjects.Add(module.name, module);

        return gameObjects;
    }

    // Generates a list of objects and their adjacency rules
    public Dictionary<string, Dictionary<Dir, List<string>>> GenerateFromSource()
    {
        Dictionary<string, Dictionary<Dir, List<string>>> moduleRules = new Dictionary<string, Dictionary<Dir, List<string>>>();

        foreach (var module in sampleModules)
            if (!moduleRules.ContainsKey(module.name))
                moduleRules.Add(module.name, GetAdjacents(module.name));

        return moduleRules;
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

        foreach (var module in sampleModules)
        {
            if (module.name == moduleName)
            {
                GameObject adjacentObject = FindObjectAtPosition(module.transform.position + Vector3.forward);
                if (adjacentObject != null && !adjacents[Dir.Forward].Contains(adjacentObject.name))
                    adjacents[Dir.Forward].Add(adjacentObject.name);

                adjacentObject = FindObjectAtPosition(module.transform.position + Vector3.back);
                if (adjacentObject != null && !adjacents[Dir.Back].Contains(adjacentObject.name))
                    adjacents[Dir.Back].Add(adjacentObject.name);

                adjacentObject = FindObjectAtPosition(module.transform.position + Vector3.left);
                if (adjacentObject != null && !adjacents[Dir.Left].Contains(adjacentObject.name))
                    adjacents[Dir.Left].Add(adjacentObject.name);

                adjacentObject = FindObjectAtPosition(module.transform.position + Vector3.right);
                if (adjacentObject != null && !adjacents[Dir.Right].Contains(adjacentObject.name))
                    adjacents[Dir.Right].Add(adjacentObject.name);

                adjacentObject = FindObjectAtPosition(module.transform.position + Vector3.up);
                if (adjacentObject != null && !adjacents[Dir.Up].Contains(adjacentObject.name))
                    adjacents[Dir.Up].Add(adjacentObject.name);

                adjacentObject = FindObjectAtPosition(module.transform.position + Vector3.down);
                if (adjacentObject != null && !adjacents[Dir.Down].Contains(adjacentObject.name))
                    adjacents[Dir.Down].Add(adjacentObject.name);
            }
        }

        return adjacents;
    }

    // Gets the object at a given position
    private GameObject FindObjectAtPosition(Vector3 position)
    {
        GameObject gameObject = null;
        float maxDistance = 1;

        foreach (var module in sampleModules)
        {
            float distance = Vector3.Distance(position, module.transform.position);
            if (distance < maxDistance)
            {
                maxDistance = distance;
                gameObject = module;
            }
        }

        return gameObject;
    }
}
