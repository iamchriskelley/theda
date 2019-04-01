using System;
using UnityEngine;
using System.Collections.Generic;

public class Wire : MonoBehaviour {
    Material mat;
    Color highlight_color = new Color(1f, 0.5f, 0); //orange
    Color normal_color;
    public string ref_des;
    public float width;
    public string layer, name, net;
    List<Transform> nodes;

    void Awake()
    {
        nodes = new List<Transform>();
        mat = gameObject.GetComponent<Renderer>().material;
        normal_color = mat.color;
    }

    public bool set_layer(string lyr)
    {
        layer = lyr;
        return true;
    }

    public bool connect(Transform n0, Transform n1)
    {
        if (n0 == null || n1 == null) return false;
        nodes.Add(n0);
        nodes.Add(n1);
        return true;
    }

    public Transform first_node()
    {
        return nodes[0];
    }

    public Transform second_node()
    {
        return nodes[1];
    }
    public bool is_connected_to(Transform node)
    {
        return nodes.Contains(node);
    }

    public bool remove(Transform wire)
    {
        nodes.Remove(wire);
        return true;
    }

    public bool clear()
    {
        nodes.Clear();
        return true;
    }

    private void OnMouseDown()
    {
        Debug.Log("Hi, I'm " + gameObject.name);
        Debug.Log("My neighbors are " + nodes[0].name + " at " + nodes[0].position + " and " + nodes[1].name + " at " + nodes[1].position);
    }

    void OnMouseOver()
    {
        mat.color = highlight_color;
    }

    void OnMouseExit()
    {
        mat.color = normal_color;
    }

}
