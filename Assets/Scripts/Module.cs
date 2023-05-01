using UnityEngine;

public class Module
{
    private Vector3 position;
    private Vector3 scale;
    private GameObject module;

    public Module(Vector3 position, Vector3 scale, GameObject module)
    {
        this.position = position;
        this.scale = scale;
        this.module = module;
    }
}
