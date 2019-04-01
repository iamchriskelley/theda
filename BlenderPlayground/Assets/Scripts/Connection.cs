using UnityEngine;
using System.Collections.Generic;

//this is pretty much like Eagle's <signal> or <net>
/*FOR NOW: 
 
    -Wire game objects will be named as connectionName|wire_autoinc
    -Raycast hits on wires will thus directly refer back to parent connection
    -Info will be shared with parent connection by DeviceManager when route event happens
    -add_airwire(...) will be rolled into an add_wire() method using the airwire layer
    -Wire should have the ability to store whether the Wire touches a pin or pins.
    -That way, checking connectivity doesn't rely on imputing from position (with the floating-point errors that asks for).
*/
public class Connection
{
    //contactrefs let you revert to airwires quickly
    public List<RefPin> refpins; //these lists are really unordered though
    public List<Wire> wires;
    public List<Via> vias;
    public bool has_airwire;
    public Connection() { }
    public Connection(List<RefPin> rp, Wire w)
    {
        refpins = rp;
        //refpins.Add(rpp);
        wires = new List<Wire> { w };
    }

    public void add_refpin(RefPin p) { }
    public void remove_refpin(string part_ref, string pin_ref) { }


    public void connect_wire(RefPin rp, Wire w)
    {
        refpins.Add(rp);
        wires.Add(w);
    }

    public bool disconnect_wire(string wire_id)
    {
        for (int i = 0; i < wires.Count; i++)
        {
            if (wires[i].ref_des == wire_id)
            {
                Debug.Log("wire to remove is at index " + i);
                wires.RemoveAt(i);
                refpins.RemoveAt(i);
                Debug.Log(wires.Count + ", " + refpins.Count);
            }
        }
        if (wires.Count == 0)
        {
            Debug.Log("Connection is now empty and should be removed...");
            return true;
        }
        return false;
    }
}