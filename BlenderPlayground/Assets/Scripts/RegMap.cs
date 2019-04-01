using System;
using System.Collections.Generic;

[Serializable]
public class RegMap
{
    public int register_length, byte_length, byte_count;
    public string[] register_names;
    public List<RegisterByte> dataList;
    public RegMap(int register_len, int byte_len, int byte_cnt, string[] reg_names, List<RegisterByte> _dataList)
    {
        register_length = register_len;
        byte_length = byte_len;
        byte_count = byte_cnt;
        register_names = reg_names;
        dataList = _dataList;
    }
}
