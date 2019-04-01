using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour {

    public string name, net;
    List<Transform> wires;
    
	void Awake()
    {
        wires = new List<Transform>();
	}
    
    public bool connect_wire(Transform wire)
    {
        wires.Add(wire);
        return true;
    }

    public bool is_connected_to(Transform wire)
    {
        return wires.Contains(wire);
    }

    public bool remove_wire(Transform wire)
    {
        wires.Remove(wire);
        return true;
    }

    public bool clear_wires()
    {
        wires.Clear();
        return true;
    }

}
