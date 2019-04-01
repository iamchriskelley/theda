using UnityEngine;
using System.Collections;

public class GlobalsManager : MonoBehaviour {
    public string WIRE_CONNECTION_PREFIX = "__";
    public string NODE_CONNECTION_PREFIX = "..";
    public string PIN_AND_PAD_PREFIX = ".";
    public int DEFAULT_WIRE_NAME_INDEX = 1;
    public int DEFAULT_CONNECTION_NAME_INDEX = 1;
    public float DEFAULT_TRACE_HEIGHT = 0.05f;
    public char CONNECTION_PAIR_DIV = '|';
    public char CONNECTION_PIN_DIV = ':';
    public string OVERLAY_NAME = "#overlay";
    public string OVERLAY_PREFIX = "#";
    public float OVERLAY_HEIGHT = 0.05f;
    public float IOTA = 0.01f;
    public float AIRWIRE_WIDTH = 0.1f;
    public string GRID_UNITS = "mm"; //inches, later

    //public Hashtable globals;

}
