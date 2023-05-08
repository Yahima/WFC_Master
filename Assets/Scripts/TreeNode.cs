using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{
    public Vector2Int Cell { get; set; }
    public List<string> ValidTypes { get; set; }
    public string Type { get; set; }
    public List<TreeNode> Children { get; set; }
    public TreeNode Parent { get; set; }

    public TreeNode(Vector2Int cell, List<string> validTypes)
    {
        Cell = cell;
        ValidTypes = validTypes;
        Children = new List<TreeNode>();
        Parent = null;
    }

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
        child.Parent = this;
    }

    public void RemoveChild(TreeNode child)
    {
        Children.Remove(child);
        child.Parent = null;
    }
}