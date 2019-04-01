using UnityEngine;

public class RefPin
{
    public string part_ref { get; set; }
    public string pin_ref { get; set; }  //this can be a pin or a pad
    public Vector3 connection_position; 
    public RefPin() { part_ref = ""; pin_ref = ""; }
    public RefPin(string _pin_ref, string _part_ref, Vector3 con_pos)
    {
        pin_ref = _pin_ref;
        part_ref = _part_ref;
        connection_position = con_pos;
    }
}
