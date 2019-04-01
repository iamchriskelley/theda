using System;
using System.Collections.Generic;


[Serializable]
public class ConnectsList
{
    public string name;
    public List<string> connects;
    public ConnectsList(string _name, List<string> _connects)
    {
        name = _name;
        connects = _connects;
    }
}
