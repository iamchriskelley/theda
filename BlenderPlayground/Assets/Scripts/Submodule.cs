using UnityEngine;
using System.Collections.Generic;

public class Submodule
{
    public string name;
    public string ref_des;
    public int part_count;
    public List<string> part_refs, part_names, part_values, part_positions, part_rotations;
    public List<ConnectsList> connections;
    public Dictionary<string, string> attributes;
    public List<string> error_log;
    public Submodule(string _name, string _ref_des, int count, List<string> refs, List<string> names, List<string> values, List<string> positions, List<string> rotations, List<ConnectsList> conns, List<Attribute> attr)
    {
        name = _name;
        ref_des = _ref_des;
        part_count = count;
        part_refs = refs;
        part_names = names;
        part_values = values;
        part_positions = positions;
        part_rotations = rotations;
        connections = conns;

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
