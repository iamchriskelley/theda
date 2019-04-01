using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PartDisplayManager : MonoBehaviour {
    

	// Use this for initialization
	void Start () {
        Debug.Log("Part display manager loaded...");
	}

    public void display_regmaps(Dictionary<string, Part> ptd, Dictionary<string, Placement> pld)
    {
        foreach (KeyValuePair<string, Placement> kvp in pld)
        {
            Placement pl = kvp.Value;
            if (pl.has_overlay)
            {
                Transform overlay = pl.g_o.transform.Find(gameObject.GetComponent<GlobalsManager>().OVERLAY_NAME);
                overlay.gameObject.SetActive(true);
                buildRegMapGrid(pl.name, overlay, ptd[pl.name].regmap, overlay.localScale, overlay.position, overlay.rotation);
            }
        }
    }

    public void hide_displays(Dictionary<string, Placement> pld)
    {
        foreach(KeyValuePair<string, Placement> kvp in pld)
        {
            Placement pl = kvp.Value;
            if (pl.has_overlay)
            {
                pl.g_o.transform.Find(gameObject.GetComponent<GlobalsManager>().OVERLAY_NAME).gameObject.SetActive(false);
            }
        }
        Debug.Log("hide displays");
    }

    private int buildRegMapGrid(string part_name, Transform parent_T, RegMap rm, Vector3 scale, Vector3 location, Quaternion rotation, int space = 1, int pad = 0)
    {
        Vector2 sizer = new Vector2(scale.x, scale.z);
        //default aspect ratio sanity rules: ratio shouldn't exceed 1.5:1
        //if(sizer.x / sizer.y > 1.5) { sizer.x = 1.5f * sizer.y; }
        //else if (sizer.y / sizer.x > 1.5) { sizer.y = 1.5f * sizer.x; }


        Vector3 nudge = new Vector3(0, gameObject.GetComponent<GlobalsManager>().OVERLAY_HEIGHT + gameObject.GetComponent<GlobalsManager>().IOTA, 0);
        if (sizer.x <= 0 || sizer.y <= 0) { Debug.Log("Error: cannot build map with non-positive dimensions..."); return -1; }
        GameObject grid_canvas = Instantiate(Resources.Load("register_map_canvas", typeof(GameObject))) as GameObject;
        GameObject border_panel = grid_canvas.transform.Find("border_panel").gameObject;

        grid_canvas.transform.SetParent(parent_T);

        grid_canvas.name = part_name + "_regmap";
        grid_canvas.GetComponent<CanvasGroup>().alpha = 1;
        grid_canvas.GetComponent<RectTransform>().position = location + nudge;
        grid_canvas.GetComponent<RectTransform>().rotation = rotation;
        grid_canvas.GetComponent<RectTransform>().Rotate(new Vector3(90,90,0));
        grid_canvas.GetComponent<RectTransform>().sizeDelta = sizer;    //kudos to https://forum.unity.com/threads/modify-the-width-and-height-of-recttransform.270993/

        Debug.Log(scale + ", " + sizer);

        GridLayoutGroup ig = border_panel.GetComponent<GridLayoutGroup>();
        RectTransform igrt = ig.transform.GetComponent<RectTransform>();
        float rows = rm.byte_length + 1;
        float cols = rm.byte_count + 1;
        float w = igrt.rect.width;// * grid_canvas.GetComponent<RectTransform>().localScale.x;
        float h = igrt.rect.height;// * grid_canvas.GetComponent<RectTransform>().localScale.y;
        float padding = w / 1000f;
        float spacing = w / 1000f;
        float _w = (w - rows * padding) / rows;
        float _h = (h - cols * padding) / cols;

        ig.spacing = new Vector2(spacing, spacing);
        ig.padding = new RectOffset(pad, pad, pad, pad);
        ig.cellSize = new Vector2(_w, _h);

        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                GameObject txt = Instantiate(Resources.Load("grid_text", typeof(GameObject))) as GameObject;
                GameObject img = Instantiate(Resources.Load("grid_image", typeof(GameObject))) as GameObject;
                txt.transform.SetParent(img.transform);
                txt.transform.localPosition = Vector3.zero;
                txt.transform.localScale = new Vector3(_w / txt.GetComponent<RectTransform>().sizeDelta.x, _h / txt.GetComponent<RectTransform>().sizeDelta.y, 1f);
                txt.name = "txt_" + i + "_" + j;

                if (i == 0)
                {
                    if (j == 0)
                    {
                        txt.GetComponent<Text>().text = "Reg\nMap";
                    }
                    else
                    {
                        txt.GetComponent<Text>().text = rm.register_names[j - 1];
                    }
                    img.GetComponent<Image>().color = Color.gray;
                    txt.GetComponent<Text>().fontStyle = FontStyle.Bold;
                }
                else if (j == 0)
                {
                    if (i != 0) txt.GetComponent<Text>().text = "bit\n" + (rows - i - 1);
                    img.GetComponent<Image>().color = Color.gray;
                    txt.GetComponent<Text>().fontStyle = FontStyle.Bold;
                }
                else
                {
                    string d = rm.dataList[j - 1].bits[i - 1];
                    int l = d.Length;
                    if (d.Length < 4)
                    {
                        for (int z = 0; z < 4 - l; z++) { d = " " + d + " "; }//; if (l == 2) Debug.Log("::" + d + "::"); }
                        d += ".";
                    }
                    txt.GetComponent<Text>().text = "0\n" + d;
                }

                img.transform.SetParent(border_panel.transform);
                img.name = "img_" + i + "_" + j;
            }

        }

        foreach (Transform t in border_panel.GetComponentInChildren<Transform>())
        {
            t.localRotation = Quaternion.identity;
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, 0f);
        }
        return 0;
    }

}