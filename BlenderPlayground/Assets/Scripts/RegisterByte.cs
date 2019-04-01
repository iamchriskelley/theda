using System;

[Serializable]
public class RegisterByte
{
    public string[] bits;
    public RegisterByte(string[] _bits)
    {
        bits = _bits;
    }
}