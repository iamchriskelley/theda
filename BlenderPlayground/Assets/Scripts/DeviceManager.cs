using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

public class DeviceManager : MonoBehaviour {

    public string CURRENT_COMMAND = "";
    bool DO_ONCE = false;

    JSON_Manager jm;
    GlobalsManager gm;
    RouteMap rm;

    public GameObject place_marker;
    public GameObject plane;
    public GameObject message_text;
    public Button btn_add_wire, btn_remove_wire;
    public GameObject grid_manager, routes_manager, parts_manager;

    protected string OVERLAY_DISPLAY = "none";
    protected GameObject DRAG_OBJECT;
    protected Vector3 DRAG_OFFSET;

    Dictionary<string, Color> pin_colors;
    Dictionary<string, Part> parts_catalog;
    Dictionary<string, Submodule> submodules_catalog;
    Dictionary<string, Placement> placement_catalog;
    Dictionary<string, Recipe> recipes_catalog;
    Dictionary<string, Connection> connection_catalog;
    Dictionary<string, Wire> wire_catalog;

    Dictionary<string, string> commands_catalog;

    public string parts_filepath, submodules_filepath;

    public void do_sth()
    {
        if (OVERLAY_DISPLAY == "none") { OVERLAY_DISPLAY = "regmap"; }
        else { OVERLAY_DISPLAY = "none"; }
        set_overlays(OVERLAY_DISPLAY);
    }

    public void set_overlays(string to_display)
    {
        if (to_display == "none")
        {
            gameObject.GetComponent<PartDisplayManager>().hide_displays(placement_catalog);
        }
        else if (to_display == "regmap")
        {
            gameObject.GetComponent<PartDisplayManager>().display_regmaps(parts_catalog, placement_catalog);
        }
    }

    private void Start()
    {
        CURRENT_COMMAND = "move";
        gm = GetComponent<GlobalsManager>();
        jm = GetComponent<JSON_Manager>();
        rm = routes_manager.GetComponent<RouteMap>();
        rm.update_route_type(0);
        grid_manager.GetComponent<Grid>().setGridSize(0.5f);

        parts_filepath = "parts.json";
        submodules_filepath = "submodules.json";
        btn_add_wire.GetComponent<Button>().onClick.AddListener(btnAddWirePress);
        btn_remove_wire.GetComponent<Button>().onClick.AddListener(btnRemWirePress);

        wire_catalog = new Dictionary<string, Wire>();
        parts_catalog = new Dictionary<string, Part>();
        recipes_catalog = new Dictionary<string, Recipe>();
        placement_catalog = new Dictionary<string, Placement>();
        connection_catalog = new Dictionary<string, Connection>();
        submodules_catalog = new Dictionary<string, Submodule>();

        load_commands(ref commands_catalog);
        setPinColors(ref pin_colors);
        loadPartsCatalog(parts_filepath, ref parts_catalog);
        loadSubmodules(submodules_filepath, ref submodules_catalog);
        rm.setup();

        //**********
        get_recipes("recipes.json", ref recipes_catalog);
        make_recipe("atmega328 breakout SMD", ref recipes_catalog, ref placement_catalog, ref parts_catalog, ref submodules_catalog);
        make_recipe("my first board recipe", ref recipes_catalog, ref placement_catalog, ref parts_catalog, ref submodules_catalog);
        rm.initialize();    //build the initial internal graph of connections (wire list, node list, adjacency list of list)
        Debug.Log(Environment.Version.ToString());

        
        String jsonString = "{'PlayerName': 'Frank','Age': 36}";
        var jo = JObject.Parse(jsonString);
        Debug.Log(jo);

        jo = new JObject();
        jo.Add("PlayerName", "Frank");
        jo.Add("Age", 36);
        //jo.Add("Stats", JToken.FromObject(stats));
        var json = jo.ToString();
        //Debug.Log(json);

        string filePath = Path.Combine(Application.streamingAssetsPath, "parts.json");  // Path.Combine combines strings into a file path at Assets/StreamingAssets
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath); // Read the json from the file into a string
            var ob = JObject.Parse(dataAsJson);
            Debug.Log(ob);
        }
        else
        {
            Debug.LogError("Cannot load submodules data!");
        }


        //**********
        //add_device(new Placement("RTC1", "MCP79412", "SOIC8", "", new Vector3(8f, .19f, -5f), new Vector3(0, 0, 0)));
    }

    void get_recipes(string filename, ref Dictionary<string, Recipe> rd)
    {
        List<Recipe> rl = jm.getRecipesJSON(filename);
        foreach (Recipe r in rl)
        {
            rd.Add(r.name, r);
        }
    }

    int make_recipe(string name, ref Dictionary<string, Recipe> rd, ref Dictionary<string, Placement> pld, ref Dictionary<string, Part> ptd, ref Dictionary<string, Submodule> smd)//,ref Dictionary<string, Module> md)
    {
        Dictionary<string, string> ref_des_updates = new Dictionary<string, string>();
        if (!rd.ContainsKey(name))
        {
            Debug.Log("Error: recipe not found");
            return 0;
        }
        foreach (KeyValuePair<string, IngredientJSON> kvp in rd[name].ingredients)
        {
            string nm = kvp.Key;
            IngredientJSON ing = kvp.Value;
            if (ing.type == "submodule" && smd.ContainsKey(nm))
            {
                add_submodule(submodules_catalog[nm], ptd, new Vector2(ing.position[0], ing.position[1]), ing.rotation, ing.ref_des);
            }
            else if (ing.type == "part" && ptd.ContainsKey(nm))
            {
                add_device(new Placement(ing.ref_des, ing.name, ptd[ing.name].package, ptd[ing.name].value, new Vector3(ing.position[0], 0, ing.position[1]), new Vector3(0f, ing.rotation, 0f)));
            }

        }

        parse_part_connections("recipe", rd[name].connections);  //turn the connections string into airwires and end nodes properly nested in empty net objects
        //rm.add_connection("foo", "bar");    //add a dummy connection which should through an error assuming no pins are actually named foo and bar.
        return 1;
    }

    /*---NOTES SECTION-----/
    In normal 3D view, have floating 3D pin labels that track to face camera
    In place of schematic capture mode, have a schematic view which is a 2D top-down viewpoint with schematic symbols overlays atop the placements. In this view/mode, nets just follow the existing routes and airwires (for now).
    -> Eventually, the schematic mode should allow for user edits to position of schematic symbols without affecting board outlay (i.e. Placements and Wires should have an updatable schematic display position)

        Next:
        -support for drawing airwires
        -ability to remove Placements
        DONE! -highlight wires on hover

        -menu for importing parts and submodules
        -basic routing/ripup tool (top only is okay for first demo)
        -add in TQFP32, X SOT23-5, and 0.1" header pins
        
        -basic proof-of-concept Eagle export function
        -updated footprints
        -dynamic runtime adjustment of footprints and parts

        -basic schematic overlay view
        -reduce schematic connections (including airwires) to labeled orbs at each RefPin which highlight the whole Connection when hovered over / tapped.

    /--------------------*/
    void Update()
    {
        get_mode();
        if (CURRENT_COMMAND == "route")
        {
            if (DO_ONCE)
            {
                //reduce_colliders();
                DO_ONCE = false;
            }
            rm.handle_routing_demo();
        }
        //handle_board();
        //router.update_route_style();
    }

    void reduce_colliders()
    {
        Debug.Log("reducing colliders");
        foreach (Transform child in parts_manager.transform)
        {
            child.GetComponent<device_style>().reduce_colliders();
        }
    }

    bool wrap_up(string old_command)
    {
        if (old_command == "route")
        {
            Debug.Log("need to re-expand colliders...");
            foreach (Transform child in parts_manager.transform)
            {
                child.GetComponent<device_style>().expand_colliders();
            }
        }
        return true;
    }

    void get_mode()
    {
        if (Input.anyKeyDown)
        {
            string s = "";
            if (Input.GetKey(KeyCode.Escape)) { s = "esc"; }
            else if (Input.inputString != "") { s = Input.inputString; s = s[s.Length - 1].ToString(); }
            if (s != "")
            {
                if (commands_catalog.ContainsKey(s) && commands_catalog[s] != CURRENT_COMMAND)
                {
                    wrap_up(CURRENT_COMMAND);
                    CURRENT_COMMAND = commands_catalog[s];
                    Debug.Log(CURRENT_COMMAND);
                    DO_ONCE = true;
                }
            }
        }
    }

    void handle_board() {
        if (Input.GetKey(KeyCode.Escape))
        {
            set_message_text("");
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000);

        if (hits.Length > 0)
        {
            //Debug.Log("RAYCAST:");
            foreach (RaycastHit h in hits)
            {
                var go = h.transform.gameObject;
                if (!is_wire(go))//is_movable(h.transform.gameObject))
                {
                    if (is_pin(go)) { go = go.transform.parent.gameObject; }
                    //Debug.Log("\t" + go.name);
                }
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            if (Physics.Raycast(ray, out hit))
            {
                var go = hit.transform.gameObject;
                if (is_wire(go))
                {
                    remove_wire(go);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            GameObject go = null;
            if (Input.GetMouseButton(0)) { go = DRAG_OBJECT; }
            else
            {
                hits = Physics.RaycastAll(ray, 1000);
                if (hits.Length > 0)
                {
                    Debug.Log("RAYCAST:");
                    foreach (RaycastHit h in hits)
                    {
                        go = h.transform.gameObject;
                        if (!is_wire(go))
                        {
                            if (is_pin(go) || is_overlay(go))
                            {
                                go = go.transform.parent.gameObject;
                            }
                        }
                    }
                }
            }
            if (go != null)
            {
                go.transform.Rotate(new Vector3(0, 90, 0));
                update_wires(go);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            hits = Physics.RaycastAll(ray, 1000);
            if (hits.Length > 0)
            {
                Debug.Log("RAYCAST:");
                foreach (RaycastHit h in hits)
                {
                    var go = h.transform.gameObject;
                    if (!is_wire(go))
                    {
                        if (is_pin(go) || is_overlay(go))
                        {
                            DRAG_OBJECT = go.transform.parent.gameObject;
                        }
                        else { DRAG_OBJECT = go; }
                        Debug.Log("\t" + go.name);
                        DRAG_OFFSET = DRAG_OBJECT.transform.position - h.point;
                        plane.layer = LayerMask.NameToLayer("Default");
                    }
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                //if (is_movable(hit.transform.gameObject))
                //{
                if (DRAG_OBJECT != null) {
                    //Debug.Log(DRAG_OBJECT + ", " + hit.point);
                    DRAG_OBJECT.transform.position = new Vector3(hit.point.x + DRAG_OFFSET.x, DRAG_OBJECT.transform.position.y, hit.point.z + DRAG_OFFSET.z);
                    update_wires(DRAG_OBJECT);
                }
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (DRAG_OBJECT != null)// && is_movable(hit.transform.gameObject))
                {
                    plane.layer = LayerMask.NameToLayer("Ignore Raycast");
                    update_wires(DRAG_OBJECT);
                    DRAG_OBJECT = null;
                }
            }
        }
    }

    public void set_message_text(string s)
    {
        message_text.GetComponent<Text>().text = s;
    }

    public void btnAddWirePress()
    {
        set_message_text("add wire");
    }

    public void btnRemWirePress()
    {
        set_message_text("remove wire");
    }


    void add_device(Placement pl, bool autoadd_passives = true)
    {
        if (parts_catalog.ContainsKey(pl.name))
        {
            //Debug.Log("ADD_DEVICE: " + pl.name + pl.ref_des);
            Part pt = parts_catalog[pl.name];
            pl.setGameObject(Instantiate(Resources.Load("components/" + pl.package, typeof(GameObject))) as GameObject);
            pl.g_o.name = pl.ref_des;
            pl.g_o.AddComponent<device_style>();
            pl.g_o.GetComponent<device_style>().set_device(pt.name, "", pt.type, "", pt.package, pt.pin_count, pt.pins, pt.pin_classes);
            pl.g_o.transform.position += pl.location;
            pl.g_o.transform.rotation = pl.g_o.GetComponent<device_style>().getBaseQuat();//Quaternion.identity;Quaternion.identity;
            pl.g_o.transform.Rotate(pl.rotation);
            pl.g_o.transform.SetParent(parts_manager.transform);
            //Debug.Log("Adding device at: " + pl.g_o.transform.position.ToString());
            foreach (Transform child in pl.g_o.GetComponentInChildren<Transform>())
            {
                var n = child.name;
                if (pt.pins.Contains(n))
                {
                    if (pl.g_o.transform.Find(gm.PIN_AND_PAD_PREFIX + n) != null)
                    {
                        Vector3 pos = pl.g_o.transform.Find(gm.PIN_AND_PAD_PREFIX + n).transform.position;
                        Vector3 locpos = pl.g_o.transform.Find(gm.PIN_AND_PAD_PREFIX + n).transform.localPosition;
                        //Debug.Log("footprint centroid for " + n + ": " + pos.ToString() + "; " + locpos.ToString());
                    }
                    else
                    {
                        Debug.Log("WARNING: no footprint found for " + n);
                    }
                }
            }

            if (autoadd_passives)
            {
                if (pt.attributes.ContainsKey("has_passives") && pt.attributes["has_passives"] == "true")
                {
                    //Debug.Log("auto-adding passives...");
                    if (pt.attributes.ContainsKey("decoupling_capacitor")) {
                        //Debug.Log("decoupling cap: " );
                        string[] parts = pt.attributes["decoupling_capacitor"].Split('.');
                    }
                }
            }
            placement_catalog.Add(pl.ref_des, pl);

            //add register map to overlay
            pl.has_overlay = false;
            pl.has_regmap = false;
            Debug.Log(pl.name + ", " + pl.g_o.name);
            if (pl.g_o.transform.Find("#overlay") ?? null != null) { pl.has_overlay = true; }
            if (parts_catalog[pl.name].regmap != null)
            {
                pl.has_regmap = true;
                //gameObject.GetComponent<PartDisplayManager>().BuildRegMaps(parts_catalog[pl.name], pl);
            }
        }
        else
        {
            Debug.Log("Can't add " + pl.name + "...part not found in catalog");
        }
    }

    void add_submodule(Submodule s, Dictionary<string, Part> pd, Vector2 location, float rotation, string ref_des_override = null)
    {
        Vector3 anchor_pos = new Vector3(0, 0, 0);//, rot = new Vector3(0, 0, 0);
        List<Placement> pls = new List<Placement>();
        for (int i = 0; i < s.part_count; i++)
        {
            Vector3 pos = new Vector3(0, 0, 0), rot = new Vector3(0, 0, 0);
            if (i == 0)
            {
                pos = xy_string_to_vector3(s.part_positions[i]) + new Vector3(location.x, 0, location.y);
                //Debug.Log(pos);
                rot = new Vector3(0, float.Parse(s.part_rotations[i]), 0);
                anchor_pos = pos;
            }
            else if (i > 0 && s.attributes.ContainsKey("offset_from_first"))
            {
                Vector3 local = xy_string_to_vector3(s.part_positions[i]);
                double x = local.x, z = local.z, r = -rotation * Math.PI / 180.0, x1, z1;
                //Debug.Log(x + "," + z);
                x1 = x * Math.Cos(r) - z * Math.Sin(r);
                z1 = z * Math.Cos(r) + x * Math.Sin(r);
                //Debug.Log(x1 + "," + z1);
                pos = new Vector3(location.x, 0, location.y) + new Vector3((float)x1, 0, (float)z1);
                rot = new Vector3(0, float.Parse(s.part_rotations[i]), 0);
                //Debug.Log(pos);

                if (s.attributes["offset_from_first"].ToLower() == "true")
                {
                    //pos += string_to_vector3(s.part_positions[0]);
                    //rot += string_to_vector3(s.part_rotations[0]);
                }
            }
            else
            {
                pos = xy_string_to_vector3(s.part_positions[i]) + new Vector3(location.x, 0, location.y);
                //Debug.Log(pos);
                rot = new Vector3(0, float.Parse(s.part_rotations[i]), 0);
                anchor_pos = pos;
            }
            pls.Add(new Placement(s.part_refs[i], s.part_names[i], pd[s.part_names[i]].package, pd[s.part_names[i]].value, pos, rot));
            //Debug.Log(pls[s.part_refs[i]].g_o.name);
        }
        string submod_ref_des = s.ref_des;
        if (ref_des_override != null) submod_ref_des = ref_des_override;
        add_placements(submod_ref_des, pls, s.connections);
    }

    string get_new_ref_des(string ref_des, Dictionary<string, Placement> pd)
    {
        if (!pd.ContainsKey(ref_des)){ return ref_des; }
        else
        {
            int index = 1;
            //int attempts = 1;
            while(pd.ContainsKey(ref_des + "_" + index) && index < int.MaxValue)
            {
                ++index;
                //++attempts;
            }
            return ref_des + "_" + index;
        }
    }

    void add_placements(string overname, List<Placement> pls, List<ConnectsList> connections = null)
    {
        //string anchor_name = pls[0].name;
        //Vector3 anchor_loc = pls[0].location;
        //Vector3 anchor_rot = pls[0].rotation;
        if (connections == null) { connections = new List<ConnectsList>(); }
        Dictionary<string, Placement> d_temp = new Dictionary<string, Placement>();
        foreach (Placement pl in pls) { d_temp[pl.ref_des] = pl; }

        foreach (Placement pl in pls)
        {
            if (parts_catalog.ContainsKey(pl.name))
            {
                //add Part
                pl.ref_des = get_new_ref_des(pl.ref_des, placement_catalog);
                Part pt = parts_catalog[pl.name];
                Debug.Log(pl.ref_des + ", " + pl.name + ", " + pl.location);
                pl.setGameObject(Instantiate(Resources.Load("components/"+pl.package, typeof(GameObject))) as GameObject);
                pl.g_o.name = pl.ref_des;
                pl.g_o.AddComponent<device_style>();
                pl.g_o.GetComponent<device_style>().set_device(pt.name, "", pt.type, "", pt.package, pt.pin_count, pt.pins, pt.pin_classes);
                pl.g_o.transform.position = pl.location;
                pl.g_o.transform.rotation = pl.g_o.GetComponent<device_style>().getBaseQuat();//Quaternion.identity;
                pl.g_o.transform.Rotate(pl.rotation);
                pl.g_o.transform.SetParent(parts_manager.transform);
                foreach (Transform child in pl.g_o.GetComponentInChildren<Transform>())
                {
                    var n = child.name;
                    if (pt.pins.Contains(n))
                    {
                        if (pl.g_o.transform.Find(gm.PIN_AND_PAD_PREFIX + n) != null)
                        {
                            Vector3 pos = pl.g_o.transform.Find(gm.PIN_AND_PAD_PREFIX + n).transform.position;
                            Vector3 locpos = pl.g_o.transform.Find(gm.PIN_AND_PAD_PREFIX + n).transform.localPosition;
                        }
                        else
                        {
                            Debug.Log("no footprint found for " + n);
                        }
                    }
                }

                placement_catalog.Add(pl.ref_des, pl);

            }
            else
            {
                Debug.Log("Can't add " + pl.name + "...part not found in catalog.");
            }
        }
        parse_part_connections(overname, connections);
    }
    
    void parse_part_connections(string overname, List<ConnectsList> connections){
        //Debug.Log("overname: " + overname);
        foreach (ConnectsList cl in connections)
        {
            foreach(string pin_string in cl.connects)
            {
                string module = "", submodule = "", part = "", pin = "";
                string net_name = gm.NODE_CONNECTION_PREFIX + parse_pin_string(pin_string, ref module, ref submodule, ref part, ref pin);
                if(overname != "" && overname != null && overname != "recipe"){
                   net_name += gm.CONNECTION_PIN_DIV + overname;
                }
                //Debug.Log(net_name + ", " + part + ", " + placement_catalog[part].g_o.name);
                Vector3 npos = placement_catalog[part].g_o.transform.Find("." + pin).transform.position;
                if (routes_manager.transform.Find(cl.name) == null)
                {
                    GameObject new_net = new GameObject();
                    new_net.name = cl.name;
                    new_net.transform.SetParent(routes_manager.transform);
                }
                if (routes_manager.transform.Find(cl.name).transform.Find(net_name) == null)
                {
                    rm.add_node(routes_manager.transform.Find(cl.name).transform, npos, net_name);
                }
            }

        }

    }

    //for now, we're only passing two-param (part:pin) names to this function; module and submodule names are handled in the "heirarchy" string
    string parse_pin_string(string pin_string, ref string module, ref string submodule, ref string part, ref string pin)
    {
        string[] s = pin_string.Split(gm.CONNECTION_PIN_DIV);
        if(s.Length < 2) return "error";
        else if(s.Length == 2)
        {
            part = s[0];
            pin = s[1];
            return pin + gm.CONNECTION_PIN_DIV + part;
        }
        else if(s.Length == 3)
        {
            submodule = s[0];
            part = s[1];
            pin = s[2];
            return pin + gm.CONNECTION_PIN_DIV + part + gm.CONNECTION_PIN_DIV + submodule;
        }
        else if (s.Length == 4)
        {
            module = s[0];
            submodule = s[1];
            part = s[2];
            pin = s[3];
            return pin + gm.CONNECTION_PIN_DIV + part + gm.CONNECTION_PIN_DIV + submodule + gm.CONNECTION_PIN_DIV + module;
        }
        else
        {
            return "error";
        }        
    }

    GameObject add_airwire(string n, Vector3 from, Vector3 to)
    {//credit to: https://answers.unity.com/questions/21174/create-cylinder-primitive-between-2-endpoints.html
        Debug.Log("making airwire from " + from.ToString() + " to " + to.ToString());
        GameObject go = Instantiate(Resources.Load("Airwire", typeof(GameObject))) as GameObject;
        go.transform.position = to + (from - to) / 2;
        go.transform.up = (from - to);
        go.transform.localScale = new Vector3(go.transform.localScale.x, Vector3.Distance(from, to)/2, go.transform.localScale.z);
        go.name = n;
        return go;
    }

    bool is_wire(GameObject go)
    {
        if (go.name.Substring(0, 2) == gm.WIRE_CONNECTION_PREFIX) return true;
        else return false;
    }

    bool is_pin(GameObject go)
    {
        if (go.name.Length > 4 && go.name.Substring(0, 4) == gm.PIN_AND_PAD_PREFIX + "pin") return true;
        else if (go.name.Length > 4 && go.name.Substring(0, 4) == gm.PIN_AND_PAD_PREFIX + "pad") return true;
        else return false;
    }

    bool is_overlay(GameObject go)
    {
        if (go.name.Substring(0, 1) == gm.OVERLAY_PREFIX) return true;
        return false;
    }

    bool is_movable(GameObject go)
    {
        if (go == plane) return false;
        if (go.name.Substring(0, 2) == gm.WIRE_CONNECTION_PREFIX) return false;
        if (go.name.Substring(0, 1) == gm.PIN_AND_PAD_PREFIX) return false;
        if (go.name.Substring(0, 8) == gm.OVERLAY_NAME) return false;
        //Debug.Log(go.name);
        return true;
    }

    void reposition_wire(Wire w, Vector3 from, Vector3 to)
    {
        //w.g_o.transform.position = to + (from - to) / 2;
        //w.g_o.transform.up = (from - to);
        //w.g_o.transform.localScale = new Vector3(w.g_o.transform.localScale.x, Vector3.Distance(from, to) / 2, w.g_o.transform.localScale.z);
    }

    void update_wire(Wire w)
    {
        /*
        w.start = GameObject.Find(w.start_ref).transform.position;
        w.stop = GameObject.Find(w.stop_ref).transform.position;
        */
    }

    void update_wires(GameObject go)
    {
        /*
        var ws = placement_catalog[go.name].wire_connections;
        if (ws.Count > 0)
        {
            foreach (KeyValuePair<string, string> kvp in ws)
            {
                var wn = kvp.Key;
                var w = wire_catalog[wn];
                if (go.name == w.stop_ref.Split('/')[0]){  reposition_wire(w, GameObject.Find(w.stop_ref).transform.position, w.start); }
                if (go.name == w.start_ref.Split('/')[0]){ reposition_wire(w, GameObject.Find(w.start_ref).transform.position, w.stop); }
                w.start = GameObject.Find(w.start_ref).transform.position;
                w.stop = GameObject.Find(w.stop_ref).transform.position;
            }
        }*/
    }

    void remove_wire(GameObject go)
    {
        /*
        Debug.Log(wire_catalog[go.name].start_ref);
        foreach(KeyValuePair<string, Wire> kvp in wire_catalog)
        {
            Debug.Log(kvp.Key);
        }
        foreach (KeyValuePair<string, Placement> kvp in placement_catalog)
        {
            Debug.Log(kvp.Key);
        }
        placement_catalog[wire_catalog[go.name].start_ref.Split('/')[0]].disconnect_wire(go.name);
        placement_catalog[wire_catalog[go.name].stop_ref.Split('/')[0]].disconnect_wire(go.name);
        Debug.Log("Removing " + wire_catalog[go.name].ref_des);

        //if there are no more wires in the Connection, remove it from catalog
        if (connection_catalog[wire_catalog[go.name].get_connection()].disconnect_wire(go.name))
        {
            connection_catalog.Remove(wire_catalog[go.name].get_connection());
        }
        wire_catalog.Remove(go.name);
        Destroy(go);
        */
    }

    //======================================================================================
    void load_commands(ref Dictionary<string, string> cc)
    {
        cc = new Dictionary<string, string>()
        {
            {"Esc", "cancel" },
            {"a", "add" },
            {"c", "copy"},
            {"d", "delete"},
            {"e", "export"},
            {"f", "find"},
            {"g", "group"},
            {"h", "help"},
            {"i", "import"},
            {"j", "DRC"},
            {"k", "lock"},
            {"m", "move"},
            {"n", "ratsnest"},
            {"r", "route"},
            {"s", "swap"},
            {"u", "ripup" },
            {"v", "split"},
            { "z", "menu"}
        };
    }

    public Color getPinColor(string s)
    {
        if (pin_colors.ContainsKey(s)){return pin_colors[s];}
        return Color.gray;
    }
    
    public void loadPartsCatalog(string filename, ref Dictionary<string, Part> pc)
    {
        jm.parsePartsJSON(filename, ref pc);
    }

    public void loadSubmodules(string filename, ref Dictionary<string, Submodule> smd)
    {
        jm.parseSubmodulesJSON(filename, ref smd);
    }

    public void setPinColors(ref Dictionary<string, Color> pc)
    {
        Color   purple = new Color(0.58f, 0, 0.83f), 
                orange = new Color(0.607f, 0.16f, 0), 
                brown = new Color(1f, 0.752f, 0.796f);
        pc = new Dictionary<string, Color>{
            {"GND", Color.blue},{"VCC", Color.red},{"AVCC", Color.red},{"VREF", Color.red},{"AREF", Color.red},{"VBAT", Color.magenta},{"I2C", Color.yellow},{"SDA", Color.yellow},{"SCL", Color.yellow},{"SPI", Color.cyan},{"SCK", Color.cyan},
            {"MISO", Color.cyan},{"MOSI", Color.cyan},{"CS", Color.cyan},{"RX", purple},{"TX", purple},{"RTS", purple},{"CTS", purple},{"DSR", purple},{"DTR", purple},{"ANA", Color.green},{"DIG", Color.green},
            {"GPIO", Color.green},{"INT", brown},{"XTAL", orange},{"RESET", Color.white},{"EN", Color.white},{"ADDR", Color.black}, {"FREQ", Color.yellow}
        };
    }

    public static Vector3 xy_string_to_vector3(string sVector)
    {//credit to: https://answers.unity.com/questions/1134997/string-to-vector3.html
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        string[] sArray = sVector.Split(',');
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            0f,
            float.Parse(sArray[1])
        );
        //Debug.Log(result);
        return result;
    }
}
