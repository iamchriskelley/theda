using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RouteMap : MonoBehaviour
{
    GlobalsManager gm;
    private int WIRE_INDEX = 0, NODE_INDEX = 0;
    public GameObject grid_manager;
    public GameObject plane;

    //-------TEMP and RUNTIME STUFF---------
    public GameObject check1, check2;
    Transform ROUTE_START_POINT;
    GameObject TEMP_ROUTE_WIRE;
    bool AM_ROUTING = false;
    public string ROUTING_LAYER = "top";
    public string ROUTING_METHOD = "square";
    public float ROUTING_WIDTH = 0.40f;
    bool route_started = false, end_trace = false;
    private string routing_direction;
    //--------------------------

    Grid grid;
    List<string> route_types;
    int route_types_index = -1;
    Vector3 off_grid, grid_center;
    Vector3 last_position, start_position;

    public GameObject default_trace, current_trace, to_pt, from_pt, hit_pt, route_from, route_to;
    public Text display_text;

    Dictionary<string, Node> nodes;
    Dictionary<string, Wire> traces;
    List<List<string>> adjacencies;
    
    private void Awake()
    {
        gm = GetComponent<GlobalsManager>();
        nodes = new Dictionary<string, Node>(); //new List<string>();
        traces = new Dictionary<string, Wire>();//new List<string>();
        adjacencies = new List<List<string>>();

        route_types = new List<string>() { "hsquare", "diag", "direct", "vsquare", "rdiag" };
        off_grid = new Vector3(-1000, 0, -1000);
        grid_center = new Vector3(0, 0, 0);
        last_position = off_grid;
    }

    public void setup()
    {

    }

    public void route_airwire(Transform wire, Vector3 pt, string layer, float width)//Vector3 hitpt, ref Dictionary<string, Vector3> pieces)
    {
        Wire w = wire.gameObject.GetComponent<Wire>();
        Grid g = grid_manager.GetComponent<Grid>();
        Transform node1 = w.first_node(), node2 = w.second_node();
        Transform route_start = node1, route_end = node2;
        if (Vector3.Distance(pt, node1.position) > Vector3.Distance(pt, node2.position))
        {
            route_start = node2;
            route_end = node1;
        }


        //X find near node
        //X get stop point
        //insert new node (get valid name) at stop point
        Transform new_node = add_node(wire.parent, pt);
        Debug.Log("new node name: " + new_node.name);

        //move_wire(ref wire, route_start, new_node, route_end, route_end, wire.parent);



        //connect current wire from new_node to route_end and update w.connect()
        //add new wire from route_start to new_node and w.connect()
        //update wire for new_node
        //update wire for route_end
        //(wire for route_start stays the same)

        Debug.Log("Route from " + route_start.name + " to " + pt + " as " + ROUTING_LAYER);
    }


    Transform get_routing_start_point(Transform wire, Vector3 pt)
    {
        Wire w = wire.gameObject.GetComponent<Wire>();
        Transform node1 = w.first_node(), node2 = w.second_node();
        Transform route_start = node1;
        if (Vector3.Distance(pt, node1.position) > Vector3.Distance(pt, node2.position))
        {
            route_start = node2;
        }
        return route_start;
    }


    void reposition_wire(ref GameObject wire, Vector3 from, Vector3 to)
    {
        from = new Vector3(from.x, 0f, from.z);
        to = new Vector3(to.x, 0f, to.z);
        wire.transform.position = from + (to - from) / 2;
        wire.transform.localScale = new Vector3(wire.transform.localScale.x, wire.transform.localScale.y, Vector3.Distance(to, from));
        wire.transform.LookAt(to);
    }

    Vector3 raycast_to_wire(Vector3 pt, Transform wire, bool follow_grid = true)
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        Wire w = wire.gameObject.GetComponent<Wire>();
        Grid g = grid_manager.GetComponent<Grid>();
        Transform node1 = w.first_node(), node2 = w.second_node();
        Vector3 c = node1.position, f = node2.position;
        if (Vector3.Distance(pt, node1.position) > Vector3.Distance(pt, node2.position)) {
            c = node2.position;
            f = node1.position;
        }
        Vector3 d = f - c, a = off_grid, b = off_grid;
        Vector3 g1, g2;
        float x1 = 0, x2 = 0.5f;
        float d1 = 0, d2 = 0;
        for (int i = 0; i < 12; i++)
        {
            a = c + d * x1;
            b = c + d * x2;
            d1 = Vector3.Distance(pt, a);
            d2 = Vector3.Distance(pt, b);
            if (d1 > d2) { x1 += (x2 - x1) * 0.5f; }
            else { x2 -= (x2 - x1) * 0.5f; }
            g1 = g.GetNearestPointOnGrid(a);
            g2 = g.GetNearestPointOnGrid(b);
            if (g1 == g2) break;
        }
        a = (a + b) / 2.0f;
        b = g.GetNearestPointOnGrid(a);
        check1.transform.position = a;
        check2.transform.position = b;

        stopWatch.Stop();
        Debug.Log("point on wire found in " + stopWatch.Elapsed.TotalMilliseconds.ToString() + "ms");
        if (follow_grid) return b;
        else return a;
    }
    
    public void initialize()
    {
        foreach (Transform child in gameObject.transform)
        {
            List<Transform> grandkids = new List<Transform>();
            for(int i = 0; i < child.childCount; i++)
            {
                grandkids.Add(child.GetChild(i));
            }
            for(int i = 1; i < grandkids.Count; i++)
            {
                //Debug.Log(child.name + " " + i + " " + child.GetChild(i));
                string name = get_new_wire_name();
                Transform wire = add_wire(name, "airwire", gm.AIRWIRE_WIDTH, grandkids[i], grandkids[i - 1], child);
            }
        }
    }

    private string get_new_wire_name()
    {
        string name = gm.WIRE_CONNECTION_PREFIX + "W" + WIRE_INDEX++;
        while (traces.ContainsKey(name))
        {
            name = gm.WIRE_CONNECTION_PREFIX + "W" + WIRE_INDEX++;
        }
        return name;
    }

    private string get_new_node_name()
    {
        string name = gm.NODE_CONNECTION_PREFIX + "N" + NODE_INDEX++;
        while (traces.ContainsKey(name))
        {
            name = gm.NODE_CONNECTION_PREFIX + "N" + NODE_INDEX++;
        }
        return name;
    }

    public void add_connection(string node1, string node2)
    {
        Debug.Log("DUMMY STUB: connect " + node1 + " and " + node2);
    }

    public Transform add_node(Transform parent, Vector3 position, string name=null)
    {
        if (name == null) name = get_new_node_name();
        GameObject go = Instantiate(Resources.Load("routing/connection_node", typeof(GameObject))) as GameObject;
        go.transform.position = position;
        go.transform.SetParent(parent);
        go.name = name;
        go.GetComponent<Node>().name = name;
        return go.transform;
    }

    public Transform move_wire(ref Transform wire, Transform old_node1, Transform new_node1, Transform old_node2, Transform new_node2, Transform net)
    {
        //try
        //{
        Vector3 from = new_node1.position, to = new_node2.position;
        //GameObject wire = Instantiate(Resources.Load("routing/"+layer, typeof(GameObject))) as GameObject;
        wire.transform.position = to + (from - to) / 2;
        wire.transform.up = (from - to);
        wire.transform.localScale = new Vector3(wire.transform.localScale.x, Vector3.Distance(from, to) / 2, wire.transform.localScale.z);
        Wire w = wire.GetComponent<Wire>();
        w.name = name;
        w.net = net.name;
        w.connect(new_node1, new_node2);

        //wire.transform.SetParent(net);
        Node n1_1 = old_node1.gameObject.GetComponent<Node>();
        Node n1_2 = old_node2.gameObject.GetComponent<Node>();
        Node n2_1 = new_node1.gameObject.GetComponent<Node>();
        Node n2_2 = new_node2.gameObject.GetComponent<Node>();

        n1_1.remove_wire(wire.transform);
        n1_2.remove_wire(wire.transform);

        //n2_1.net = net.name;
        //n2_2.net = net.name;
        n2_1.connect_wire(wire.transform);
        n2_2.connect_wire(wire.transform);

        
        return wire;
        //}
        //catch (Exception)
        //{
        //    return null;
        //}
    }

    public Transform add_wire(string name, string layer, float width, Transform node1, Transform node2, Transform net)
    {
        //try
        //{
        Vector3 from = node1.position, to = node2.position;
        GameObject wire = Instantiate(Resources.Load("routing/" + layer, typeof(GameObject))) as GameObject;
        wire.transform.position = to + (from - to) / 2;
        wire.transform.up = (from - to);
        wire.transform.localScale = new Vector3(wire.transform.localScale.x, Vector3.Distance(from, to) / 2, wire.transform.localScale.z);
        Wire w = wire.GetComponent<Wire>();
        w.name = name;
        w.net = net.name;
        w.set_layer(layer);
        w.connect(node1, node2);

        wire.name = name;
        wire.transform.SetParent(net);
        Node n1 = node1.gameObject.GetComponent<Node>();
        Node n2 = node2.gameObject.GetComponent<Node>();

        Debug.Log(node1.name + "\t" + node2.name);

        n1.net = net.name;
        n2.net = net.name;
        n1.connect_wire(wire.transform);
        n2.connect_wire(wire.transform);
        return wire.transform;
        //}
        //catch (Exception)
        //{
        //    return null;
        //}
    }

    private void dummy_route()
    {
        Debug.Log("Run " + get_route_type() + " route from " + route_from.transform.position + " to " + route_to.transform.position);
    }

    public string get_route_type()
    {
        return route_types[route_types_index];
    }

    public void update_route_type(int index)
    {
        route_types_index = index;
        //Debug.Log(route_types_index + " " + route_types[route_types_index]);
        display_text.text = route_types[route_types_index];
    }

    public void update_route_type()
    {
        route_types_index = ++route_types_index % route_types.Count;
        display_text.text = route_types[route_types_index];
    }

    public void update_route_style()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //Debug.Log(route_types[route_types_index]);
            update_route_type();
            //dummy_route();
        }
    }

    public void handle_routing_demo()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit) && hit.transform.name.Substring(0, 2) == gm.WIRE_CONNECTION_PREFIX)
            {
                Transform wire = hit.transform;
                if (wire.gameObject.GetComponent<Wire>().layer == "airwire")
                {   
                    Vector3 real_hit_point = raycast_to_wire(hit.point, wire);
                    //Debug.Log(hit.point + ", " + real_hit_point + " on " + wire.name + " in " + wire.parent);

                    //route_airwire(wire, real_hit_point, routing_layer, routing_width);

                    ROUTE_START_POINT = get_routing_start_point(wire, real_hit_point);//, routing_layer, routing_width);
                    TEMP_ROUTE_WIRE = Instantiate(Resources.Load("routing/" + ROUTING_LAYER + "_trace", typeof(GameObject))) as GameObject;
                    TEMP_ROUTE_WIRE.transform.localScale = new Vector3(ROUTING_WIDTH, TEMP_ROUTE_WIRE.transform.localScale.y, TEMP_ROUTE_WIRE.transform.localScale.z);
                    AM_ROUTING = true;
                    plane.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
        else if (AM_ROUTING)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log(hit.point);
                //draw a [for now Direct] route from mouse to route start point
                reposition_wire(ref TEMP_ROUTE_WIRE, ROUTE_START_POINT.position, hit.point);
            }
        }
    }

    /*
    public void handle_routing()
    {
        RaycastHit hitInfo;
        Ray ray;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (route_started)
            {
                GameObject.Destroy(current_trace);
            }
            routing = false;
            route_started = false;
            Debug.Log("routing done.");
        }
        if (!routing)
        {
            if (Input.GetMouseButtonDown(0))    //prepare to route a trace
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    last_position = grid_manager.GetComponent<Grid>().GetNearestPointOnGrid(hitInfo.point);
                    routing = true;
                    route_started = false;
                    Debug.Log("starting a " + routing_method + "trace route...");
                }
            }
        }
        else
        { //you are routing
            if (Input.GetMouseButtonDown(1))    //right-click to change layers
            {
                if (routing_layer == "top") { routing_layer = "bottom"; }
                else { routing_layer = "top"; }
                Debug.Log("layer swap!");
            }
            else if (Input.GetMouseButtonDown(0))   //click to start a new segment
            {
                route_started = false;
                Debug.Log("starting another" + routing_method + "trace route...");
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    //last_position = grid.GetNearestPointOnGrid(hitInfo.point);  //get position
                }
            }
            else if (Input.GetMouseButtonDown(2))
            {
                //layer
            }
            if (!route_started) //start routing a trace
            {
                Debug.Log("laying trace!...");
                if (routing_layer == "top")
                {
                    SetColor(current_trace, Color.red);
                }
                else if (layer == "bottom")
                {
                    SetColor(current_trace, Color.blue);
                }
                else
                {
                    Debug.Log("can't parse layer");
                }
                PlaceTrace(ref current_trace, default_trace, last_position);
                start_position = last_position;
                route_started = true;
            }
            else //continue a route operation
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    last_position = grid_manager.GetComponent<Grid>().GetNearestPointOnGrid(hitInfo.point);
                    Vector3 end_position = GetTraceVector(last_position, start_position, route_method);
                    RouteTrace(ref current_trace, start_position, end_position, hitInfo.point);
                    last_position = end_position;
                }
            }
        }
    }
    */

    private bool SetColor(GameObject go, Color color)
    {
        try
        {
            default_trace.GetComponent<Renderer>().material.color = color;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        //return false;
    }

    private void RouteTrace(ref Transform trace, Vector3 from, Vector3 to, Vector3 hit)
    {
        hit_pt.transform.position = new Vector3(hit.x, 0, hit.z);
        to_pt.transform.position = to;
        from_pt.transform.position = from;

        trace.transform.position = from + (to - from) / 2;
        trace.transform.localScale = new Vector3(trace.transform.localScale.x, trace.transform.localScale.y, Vector3.Distance(to, from));
        trace.transform.LookAt(to);
    }

    private Vector3 GetTraceVector(Vector3 position, Vector3 start_position, string route_method)
    {
        if (route_method == "square")
        {
            float lr = position.x - start_position.x;
            float ud = position.z - start_position.z;

            if (Mathf.Abs(lr) > Mathf.Abs(ud))
            {
                if (lr > 0)
                {
                    return start_position + new Vector3(lr, 0, 0);
                }
                else
                {
                    return start_position + new Vector3(lr, 0, 0);
                }
            }
            else
            {
                if (ud > 0)
                {
                    return start_position + new Vector3(0, 0, ud);
                }
                else
                {
                    return start_position + new Vector3(0, 0, ud);
                }
            }
        }
        else
        {
            Debug.Log("route method not recognized");
            return grid_center;
        }
        //return off_grid;
    }

    private void PlaceTrace(ref GameObject trace, GameObject route_prefab, Vector3 clickPoint)
    {
        var finalPosition = grid_manager.GetComponent<Grid>().GetNearestPointOnGrid(clickPoint);
        trace = GameObject.Instantiate(route_prefab) as GameObject;
        trace.transform.position = finalPosition;
        //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = nearPoint;
    }
}

