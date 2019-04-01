using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class JSON_Manager : MonoBehaviour {

    bool DATA_RECEIVED = false;

    [Serializable]
    public class ConnectionJSON
    {
        public string conn;// start, end;
        public ConnectionJSON(string _conn)
        {
            conn = _conn;
            //string[] s  = conn.Split('|');//gameObject.GetComponent<GlobalsManager>().CONNECTION_PIN_DIV);
            //s[0] = start;
            //s[1] = end;
        }

    }

    [Serializable]
    public class SignalJSON
    {
        public string signals;
        public SignalJSON(string _signals)
        {
            signals = _signals;
        }
    }

    [Serializable]
    public class RecipeJSON
    {
        public string name, id;
        public List<IngredientJSON> ingredients;
        public List<ConnectsList> connections;
        //public List<SignalJSON> signals;
        //public List<string> context;
        public RecipeJSON() { }
        public RecipeJSON(string _id, string _name, List<IngredientJSON> _ingredients, List<ConnectsList> conns)//, List<SignalJSON> sigs, List<string> _context)
        {
            id = _id;
            name = _name;
            ingredients = _ingredients;
            connections = conns;
            //signals = sigs;
        }
    }

    [Serializable]
    public class RecipeSpecJSON
    {
        public int recipe_count;
        public List<RecipeJSON> recipes;
        public List<ConnectsList> connections;
        public RecipeSpecJSON() { }
        public RecipeSpecJSON(int _count, List<RecipeJSON> _recipes, List<ConnectsList> conns)
        {
            recipe_count = _count;
            recipes = _recipes;
            connections = conns;
        }
    }


    [Serializable]
    public class SubmoduleJSON
    {
        public string name, id, ref_des;
        public int part_count;
        public List<string> part_refs, part_names, part_values, part_positions, part_rotations;//, attribute_keys, attribute_values;
        public List<Attribute> attributes;
        public List<ConnectsList> connections;
        public SubmoduleJSON(string _id, string _name, string _ref_des, int count, List<string> refs, List<string> names, List<string> values, List<string> positions, List<string> rotations, List<ConnectsList> conns, List<Attribute> attr)//List<string> _attribute_keys, List<string> _attribute_values)
        {
            id = _id;
            name = _name;
            ref_des = _ref_des;
            part_count = count;
            part_refs = refs;
            part_names = names;
            part_values = values;
            part_positions = positions;
            part_rotations = rotations;
            connections = conns;
            attributes = attr;
            //attribute_keys = _attribute_keys;
            //attribute_values = _attribute_values;
        }
    }

    [Serializable]
    public class SubmodulesSpecJSON
    {
        public int submodule_count;
        public List<SubmoduleJSON> submodules;
        public SubmodulesSpecJSON() { }
        public SubmodulesSpecJSON(int _count, List<SubmoduleJSON> _submods)
        {
            submodule_count = _count;
            submodules = _submods;
        }
    }

    [Serializable]
    public class PartJSON
    {
        public string id, name, type, package, protocol,value;
        public int pin_count;
        public string[] pin_names, pin_classes;
        public RegMap register_map;
        public List<Attribute> attributes;
        public PartJSON(string _id, string _name, string _type, string _package, string _protocol, string _value, int _pin_count, string[] _pin_names, string[] _pin_classes, RegMap reg_map, List<Attribute> attr)
        {
            id = _id;
            name = _name;
            type = _type;
            package = _package;
            value = _value;
            pin_count = _pin_count;
            pin_names = _pin_names;
            pin_classes = _pin_classes;
            register_map = reg_map;
            attributes = attr;
        }
    }

    [Serializable]
    public class PartsSpecJSON
    {
        public int part_count;
        public List<PartJSON> parts;
        public PartsSpecJSON() { }
        public PartsSpecJSON(int _partcount, List<PartJSON> _parts)
        {
            part_count = _partcount;
            parts = _parts;
        }
    }

    public void parsePartsJSON(string filename, ref Dictionary<string, Part> pd, bool clear_parts_dict=false)
    {
        if(clear_parts_dict) {
            Debug.Log("clearing parts dictionary...");
            pd.Clear();
        }
        Debug.Log("importing parts dictionary from " + filename);
        PartsSpecJSON psj = new PartsSpecJSON();
        JSONToData(filename, ref psj);
        if (psj.part_count != psj.parts.Count) { Debug.Log("Number of parts received does not match parts manifest..."); }
        for(int i = 0; i < psj.parts.Count; i++)
        {
            PartJSON pj = psj.parts[i];
            Part p = new Part(pj.name, pj.type, pj.value, pj.package, pj.attributes, pj.pin_count, new List<string>(pj.pin_names), new List<string>(pj.pin_classes), pj.register_map);
            pd.Add(p.name, p);
        }
        
    }

    public void parseSubmodulesJSON(string filename, ref Dictionary<string,Submodule> smd)
    {
        Debug.Log("importing submodules from " + filename);
        SubmodulesSpecJSON sms = new SubmodulesSpecJSON();
        JSONToData(filename, ref sms);
        if (sms.submodule_count != sms.submodules.Count) { Debug.Log("Number of submodules received does not match submodules manifest..."); }
        for (int i = 0; i < sms.submodules.Count; i++)
        {
            SubmoduleJSON smj = sms.submodules[i];
            Submodule sm = new Submodule(smj.name, smj.ref_des, smj.part_count, smj.part_refs, smj.part_names, smj.part_values, smj.part_positions, smj.part_rotations, smj.connections, smj.attributes);
            smd.Add(sm.name, sm);
        }
    }

    public List<Recipe> getRecipesJSON(string filename)//, ref Dictionary<string, Submodule> smd)
    {
        Debug.Log("importing recipes from " + filename);
        RecipeSpecJSON rsj = new RecipeSpecJSON();
        JSONToData(filename, ref rsj);
        List<Recipe> rs = new List<Recipe>();
        if (rsj.recipe_count != rsj.recipes.Count) { Debug.Log("Number of recipes received does not match recipes manifest..."); }
        for (int i = 0; i < rsj.recipes.Count; i++)
        {
            RecipeJSON rj = rsj.recipes[i];
            Recipe r = new Recipe(rj.id, rj.name, rj.ingredients, rj.connections);
            rs.Add(r);
            //Debug.Log(rj.ingredients[0].type);
            //Debug.Log(rj.connections[0].conn);
            //Submodule sm = new Submodule(smj.name, smj.part_count, smj.part_refs, smj.part_names, smj.part_values, smj.part_positions, smj.part_rotations, smj.connections, smj.attributes);
            //smd.Add(sm.name, sm);
        }
        return rs;
    }

    /*  //This needs updating
        private void DataToJSON(ref List<RegisterByte> rbl) {        
        RegisterByte rb  = new RegisterByte(new string[] { "SEC", "SEC", "SEC", "SEC", "MIN", "MIN", "MIN", "MIN" });
        rbl.Add(rb);
        string json = JsonUtility.ToJson(rm);  Debug.Log(json);
    }*/

    private void JSONToData(string gameDataFileName, ref PartsSpecJSON pjm)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);  // Path.Combine combines strings into a file path at Assets/StreamingAssets
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath); // Read the json from the file into a string
            pjm = JsonUtility.FromJson<PartsSpecJSON>(dataAsJson);  // Pass the json to JsonUtility, and tell it to create a GameData object from it
        }
        else
        {
            Debug.LogError("Cannot load parts data!");
        }
    }

    private void JSONToData(string gameDataFileName, ref SubmodulesSpecJSON sms)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);  // Path.Combine combines strings into a file path at Assets/StreamingAssets
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath); // Read the json from the file into a string
            sms = JsonUtility.FromJson<SubmodulesSpecJSON>(dataAsJson);  // Pass the json to JsonUtility, and tell it to create a GameData object from it
        }
        else
        {
            Debug.LogError("Cannot load submodules data!");
        }
    }

    private void JSONToData(string gameDataFileName, ref RecipeSpecJSON rs)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);  // Path.Combine combines strings into a file path at Assets/StreamingAssets
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath); // Read the json from the file into a string
            rs = JsonUtility.FromJson<RecipeSpecJSON>(dataAsJson);  // Pass the json to JsonUtility, and tell it to create a GameData object from it
        }
        else
        {
            Debug.LogError("Cannot load recipe data!");
        }
    }
}
