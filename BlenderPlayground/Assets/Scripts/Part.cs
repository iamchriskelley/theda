using UnityEngine;
using System.Collections.Generic;
using System;

public class Part
{
    //name
    //type
    //tags
    //attributes
    //value
    //package
    //pin count
    //pins
    //(symbol)
    //shape
        //outlines
        //pins
    //footprint
        //pads
        //border
        //fence
        //keepout / stop
        //solder mask
    //datasheet
    //dataset
        //register map
    
    public string name { get; private set; }    //name of part, e.g. manufacturer's name
    public string type { get; private set; }    //broad category of part, e.g. capacitor
    public string value { get; private set; }   //10uF
    public string package { get; private set; } //0805
    public Dictionary<string, string> attributes { get; private set; }  //this could be user-defined attributes when the dataset container is coded in
    public List<Note> notes { get; private set; }
    public int pin_count { get; private set; }                          //e.g. 2
    public List<string> pins { get; private set; }                      //pin names
    public List<string> pin_classes { get; private set; }               //logical classes of pins, for color coding
    //public int pad_count { get; private set; }                        //I think pad variables were commented out because they are basically redundant here
    //public List<string> pads { get; private set; }
    //public List<string> pad_classes { get; private set; }
    public Vector3 location { get; set; }                       //location of part centroid
    public Vector3 rotation { get; set; }                       //1-axis rotation about centroid (relative to board normal; assuming part is parallel to board)
    public RegMap regmap { get; set; }                          //register values, if applicable -- this will be migrated to dataset container
    public List<string> error_log;                              //for when something goes wrong

    /*
     * Notes:
     * 1. Is it a good idea to skip pad definitions? While this is intended to be a layout-first, back-annotation environment, having the pads brought forward as a target for physical
     * parameters of board contact (including solder mask and keepouts) seems like a good idea.
     * 
     * //2. Seems like tags would be a good container to have, in addition to attributes, e.g. for user comments.
     * //Maybe this could be relabeled as Notes, and just be a list of Note items (which would include user id, timestamp, comment, url, etc).
    */
    public Part(string n, string t, string val, string pkg, List<Attribute> attr, int pincnt, List<string> pins_list, List<string> pins_cls, RegMap _regmap = null)
    {
        name = n;
        type = t;
        value = val;
        package = pkg;
        pins = pins_list;
        pin_count = pincnt;

        foreach(string p in pins)
        {
            Debug.Log(name + ": " + p);
        }

        pin_classes = pins_cls ?? new List<String>();
        //pad_count = padc;
        //pads = pads_list;
        //pad_classes = pads_cls ?? new List<String>();
        regmap = _regmap;

        error_log = new List<string>();
        attributes = new Dictionary<string, string>();
        foreach (Attribute a in attr)
        {
            if (attributes.ContainsKey(a.k))
            {
                error_log.Add("Duplicate attribute key: " + a.k);
            }
            else
            {
                attributes.Add(a.k, a.v);
            }
        }
    }
}
