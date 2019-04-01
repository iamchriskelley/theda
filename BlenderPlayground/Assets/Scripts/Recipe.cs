using UnityEngine;
using System.Collections.Generic;

public class Recipe
{
    public string name, id;
    public Dictionary<string, IngredientJSON> ingredients;
    //public List<string> module_list, submodule_list, parts_list;
    //public List<ComponentJSON> components;
    public List<ConnectsList> connections;
    //public List<SignalJSON> signals;
    //public List<string> context;
    public List<string> error_log;
    public Recipe() { }
    public Recipe(string _id, string _name, List<IngredientJSON> _ingredients, List<ConnectsList> conns)//, List<ConnectionJSON> conns, List<SignalJSON> sigs, List<string> _context)
    {
        id = _id;
        name = _name;
        ingredients = new Dictionary<string, IngredientJSON>();
        connections = conns;
        for(int i = 0; i < _ingredients.Count; i++)
        {
            //Debug.Log(_ingredients[i].id);
            ingredients.Add(_ingredients[i].name, _ingredients[i]);
            //if(_ingredients[i].type == "module") { module_list.Add(_ingredients[i].name); }
            //else if (_ingredients[i].type == "submodule") { submodule_list.Add(_ingredients[i].name); }
            //else if (_ingredients[i].type == "parts") { parts_list.Add(_ingredients[i].name); }
        }
        //components = comps;
        //connections = conns;
        //signals = sigs;
    }
}


