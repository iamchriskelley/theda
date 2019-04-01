using System;

[Serializable]
public class Attribute
{
    public string k, v;
    public Attribute(string _k, string _v)
    {
        k = _k;
        v = _v;
    }
}
