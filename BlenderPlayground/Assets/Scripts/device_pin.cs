using UnityEngine;
using System.Collections;

public class device_pin : MonoBehaviour
{
    Material mat;
    Color highlight_color = Color.red;
    Color normal_color;
    
    void Awake()
    {
        mat = gameObject.GetComponent<Renderer>().material;
        normal_color = mat.color;
        //Debug.Log("I've started!");
    }

    void OnMouseDown()
    {
        //drag_start_point = transform.position;
        //Debug.Log("I, " + this.name + ", have been clicked at " + drag_start_point);
    }

    void OnMouseOver()
    {
        mat.color = highlight_color;
        //Debug.Log("Mouse over " + this.name);
    }

    void OnMouseExit()
    {
        mat.color = normal_color;
        //Debug.Log("Mouse exit " + this.name);
    }
}
