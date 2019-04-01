using System;

[Serializable]
public class IngredientJSON
{
    public string id, name, type, ref_des;
    public float[] position;
    public float rotation;
    public IngredientJSON(string _id, string _ref_des, string _name, string _type, float[] pos, float rot)//, List<string> opts)
    {
        id = _id;
        name = _name;
        type = _type;
        ref_des = _ref_des;
        position = pos;
        rotation = rot;
    }
}
