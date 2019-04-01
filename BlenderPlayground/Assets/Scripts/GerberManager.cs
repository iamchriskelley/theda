using UnityEngine;
using System.Collections;

public class GerberManager : MonoBehaviour {

    GlobalsManager gm;
    string gerber_header;

    // Use this for initialization
    void Start () {
        gm = GetComponent<GlobalsManager>();
	}

    void build_simple_header(ref string gerber_header)
    {
        gerber_header = "G75*";
        if(gm.GRID_UNITS == "in")  gerber_header += "%MOIN*%";
        else gerber_header += "%MOMM*%";
        gerber_header += "%OFA0B0*%";
        gerber_header += "%FSLAX25Y25*%";
        gerber_header += "%IPPOS*%";
        gerber_header += "%LPD*%";
        //% ADD11R

    }
}
