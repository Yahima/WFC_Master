using UnityEngine;
using System.Collections.Generic;

public class Module
{
    public bool collapsed;
    public string type;
    public List<string> validTypes;
    public Dictionary<string, Dictionary<Dir, List<string>>> rules;
    public Vector3Int gridPosition;
    public GameObject model;

    public Module(Vector3Int position, List<string> types)
    {
        collapsed = false;
        gridPosition = position;
        validTypes = types;
        model = null;
    }

    public void Collapse()
    {
        System.Random random = new System.Random();
        type = validTypes[random.Next(0, validTypes.Count)];
        collapsed = true;
    }

    public void CollapseTo(string type)
    {
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

    public void SetObject(GameObject gameObject)
    {
        model = GameObject.Instantiate(gameObject, this.gridPosition, gameObject.transform.rotation);
    }
}
