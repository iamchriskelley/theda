using UnityEngine;
using System.Collections.Generic;
using System;

public class Placement
{
    public string ref_des = "";
    public string name;
    public string package;
    public string value;
    public Vector3 location { get; set; }
    public Vector3 rotation { get; set; }
    public List<string> pin_connections { get; set; }
    //List<Pair<string, string>> p = new List<Pair<string, string>>();
    public List<string> pad_connections { get; set; }
    public GameObject g_o { get; private set; }
    public Dictionary<string, string> wire_connections { get; set; }
    public bool has_overlay, has_regmap;

    public void setGameObject(GameObject go)
    {
        g_o = go;
        g_o.name = ref_des;
    }

    public Placement(string refdes, string n, string pk, string val, Vector3 loc, Vector3 rot, List<string> pincons = null, List<string> padcons = null)
    {
        ref_des = refdes;
        name = n;
        package = pk;
        value = val;
        location = loc;
        rotation = rot;
        pin_connections = pincons ?? new List<String>();
        pad_connections = padcons ?? new List<String>();
        wire_connections = new Dictionary<string, string>();
    }

    public void connect_wire(string wire_name, string other_placement_name)
    {
        if (!wire_connections.ContainsKey(wire_name)) wire_connections.Add(wire_name, other_placement_name);
        else Debug.Log("ADD Warning: " + wire_name + " already exists in " + ref_des + " wire connections.");
    }

    public void disconnect_wire(string wire_name)
    {
        if (wire_connections.ContainsKey(wire_name)) wire_connections.Remove(wire_name);
        else Debug.Log("REM Warning: " + wire_name + " not found in " + ref_des + " wire connections.");
    }
}