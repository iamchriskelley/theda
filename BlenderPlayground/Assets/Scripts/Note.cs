using UnityEngine;

public class Note
{
    public string user_id { get; set; }
    public string timestamp { get; set; }
    public string comment { get; set; }
    public string url { get; set; }

    public Note() { user_id = ""; timestamp = ""; comment = ""; url = ""; }
    public Note(string _user_id, string _ts, string _comment, string _url="")
    {
        user_id = _user_id;
        timestamp = _ts;
        comment = _comment;
        url = _url;
    }
}
