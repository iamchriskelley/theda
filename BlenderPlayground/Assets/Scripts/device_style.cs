using UnityEngine;
using System.Collections.Generic;

public class device_style : MonoBehaviour {
    Quaternion base_quat;
    Vector3 drag_start_point;
    Dictionary<string, string> pin_name;
    Dictionary<string, string> pin_type;
    public GameObject cam;
    public string name;

    Dictionary<Transform, Bounds> child_colliders;

    void Awake()
    {
        base_quat = gameObject.transform.localRotation;
        child_colliders = new Dictionary<Transform, Bounds>();
        cam = GameObject.Find("Main Camera");
        pin_name = new Dictionary<string, string>();
        pin_type = new Dictionary<string, string>();
        if (gameObject.transform.Find("#overlay") != null) { gameObject.transform.Find("#overlay").gameObject.SetActive(false); }

        foreach (Transform child in transform)
        {
            BoxCollider b = child.gameObject.GetComponent<BoxCollider>();
            if (b != null)
            {
                child_colliders.Add(child, b.bounds);
            }
        }
    }

    public Quaternion getBaseQuat()
    {
        return base_quat;
    }

    void OnMouseDown()
    {
        drag_start_point = transform.position;
        Debug.Log("I, " + this.name + ", have been clicked at " + drag_start_point);
    }

    private void OnMouseOver()
    {
        //Debug.Log("Mouse over " + this.name + " at " + drag_start_point);
    }

    public bool reduce_colliders()
    {
        foreach(KeyValuePair<Transform, Bounds> kvp in child_colliders)
        {
            Debug.Log("<stub>" + kvp.Key.name + " reducing collider to boundary from " + kvp.Value);
            //Debug.Log(kvp.Key.gameObject.GetComponent<Renderer>().bounds.extents);
            //kvp.Key.GetComponent<BoxCollider>().bounds.Encapsulate(kvp.Key.gameObject.GetComponent<Renderer>().bounds);
        }
        return true;
    }

    public bool expand_colliders()
    {
        foreach (KeyValuePair<Transform, Bounds> kvp in child_colliders)
        {
            Debug.Log("<stub>" + kvp.Key.name + " expanding collider back to full size...");
            //kvp.Key.GetComponent<BoxCollider>().bounds.Encapsulate(kvp.Value);
        }
        return true;
    }

    void OnCollisionEnter(Collision collision){Debug.Log("COLLISION DETECTED");}

    public void set_device(string _name, string device_class, string device_type, string protocol, string package, int pin_count, List<string> pin_names, List<string> pin_types) {
        name = _name;
        //Debug.Log(name + ", " + device_class + "," + device_type);
        for (int i = 1; i <= pin_count; i++)
        {
            //Debug.Log("pin" + i + ", " + pin_names[i - 1]);
            pin_name.Add("pin" + i, pin_names[i - 1]);
            pin_type.Add("pin" + i, pin_types[i - 1]);
        }

        //this.name = name;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            var n = child.name;
            if (pin_type.ContainsKey(n))
            {
                child.GetComponent<Renderer>().material.color = cam.GetComponent<DeviceManager>().getPinColor(pin_type[n]);
            }
            //else if (n != this.name){Debug.Log("pin not recognized: " + n);}
        }
    }

}