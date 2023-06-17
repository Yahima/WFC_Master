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
    private float angle;

    public Module(Vector3Int position, List<string> types)
    {
        collapsed = false;
        gridPosition = position;
        validTypes = types;
        model = null;
        angle = 0;
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

    public void SetObject(GameObject obj)
    {

        Debug.Log(type);
        int rotation = int.Parse(type[^1..]);

        Debug.Log(rotation);
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

        //Debug.Log(angle);
        model = GameObject.Instantiate(obj, this.gridPosition * new Vector3Int(10, 10, 10), Quaternion.Euler(new Vector3(0, angle, 0)));
    }

    private void Rotate()
    {
        this.model.transform.Rotate(0, 90, 0);

    }
}
