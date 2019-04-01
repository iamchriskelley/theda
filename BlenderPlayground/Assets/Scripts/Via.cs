using UnityEngine;
using System.Collections;

public class Via {
    public string ref_des { get; set; }
    public string connection;
    public Vector3 position { get; set; }
    public string shape { get; set; }
    public float outer_width { get; set; }
    public float drill_width { get; set; }
    public int layer1 { get; set; }
    public int layer2 { get; set; }
    public GameObject g_o { get; set; }

    public Via(string rfds, string conn, Vector3 pos, string _shape, float ext_width, float int_width, int lay1, int lay2, GameObject go)
    {
        ref_des = rfds;
        connection = conn;
        position = pos;
        shape = _shape;
        outer_width = ext_width;
        drill_width = int_width;
        layer1 = lay1;
        layer2 = lay2;
        g_o = go;
    }

}
