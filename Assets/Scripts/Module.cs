﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Module
{
    public bool collapsed;
    public string type;
    public List<Tuple<string, int>> typeData;
    public List<string> validTypes;
    public Dictionary<string, Dictionary<Dir, List<string>>> rules;
    public Vector3Int gridPosition;
    private bool isEdge;
    public GameObject model;

    public Module(Vector3Int position, List<Tuple<string, int>> types, bool isEdge)
    {
        collapsed = false;
        gridPosition = position;
        typeData = types;
        validTypes = new List<string>();

        foreach (var data in typeData)
            validTypes.Add(data.Item1);

        this.isEdge = isEdge;

        model = null;
    }

    public void Collapse()
    {
        System.Random random = new System.Random();

        List<string> weightedTypesList = new();
        foreach (var type in validTypes)
        {
            int weight = typeData.FirstOrDefault(tuple => tuple.Item1 == type)?.Item2 ?? 1;

            for (int i = 0; i < weight; i++)
            {
                weightedTypesList.Add(type);
            }
        }

        type = weightedTypesList[random.Next(0, weightedTypesList.Count - 1)];
        collapsed = true;
    }

    public void CollapseTo(string type)
    {
        this.type = type;
        collapsed = true;
    }

    public void CollapseToType(string type)
    {
        ResetModule();
        this.type = type;
        collapsed = true;
    }

    public bool CollapseOther(string type)
    {
        ResetModule();

        if (validTypes.Count > 1)
        {
            List<string> newTypes = validTypes;
            newTypes.Remove(type);

            System.Random random = new System.Random();
            this.type = newTypes[random.Next(0, newTypes.Count)];
            collapsed = true;
        }

        return collapsed;
    }

    public void ResetModule()
    {
        collapsed = false;
        if (model != null)
            GameObject.Destroy(model);

        model = null;
    }

    public void SetObject(GameObject obj)
    {
        int rotation = int.Parse(type[^1..]);
        float angle =  0;

        switch (rotation)
        {
            case 0:
                angle = 0;
                break;
            case 1:
                angle = 90;
                break;
            case 2:
                angle = 180; ;
                break;
            case 3:
                angle = 270;
                break;
            default:
                break;
        }

        model = GameObject.Instantiate(obj, this.gridPosition * new Vector3Int(10, 10, 10), Quaternion.Euler(new Vector3(0, angle, 0)));
    }

    public List<string> GetValidTypes()
    {
        return validTypes;
    }

    public void RemoveType(string type)
    {
        validTypes.Remove(type);
    }

    public string GetTileType()
    {
        return type;
    }

    public bool IsCollapsed()
    {
        return collapsed;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    public void SetValidTypes(List<string> types)
    {
        validTypes = types;
    }

    public bool IsEdge()
    {
        return isEdge;
    }
}
