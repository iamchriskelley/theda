using UnityEngine;
using System.Collections.Generic;
using System.IO.Ports;

public class AVRDudeManager : MonoBehaviour
{
    List<string> avrdude_output;
    public string avrdude_path = "C:/Program Files (x86)/Arduino/hardware/tools/avr/bin/avrdude.exe";
    public string conf_path = "C:/Program Files (x86)/Arduino/hardware/tools/avr/etc/avrdude.conf";
    public bool verbose = true;
    public string processor = "atmega328p";
    public string programmer = "arduino";
    public int baud = 115200;
    public string com_port = "";
    public string hexfile = "Blink.cpp.hex";
    public string memtype;

    private void Start()
    {
        hexfile = Application.dataPath + "/External Code/" + hexfile;
        avrdude_output = new List<string>();
        com_port = get_serial_port();
        upload_program();
    }

    public string get_serial_port()
    {
        string[] ports = SerialPort.GetPortNames();
        foreach (string port in ports)
        {
            if(port != "COM0")
            {
                Debug.Log("Using: " + port);
                return port;
            }
        }
        return "error";
    }

    void DataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if(e.Data != null)
        {
            avrdude_output.Add(e.Data);
        }
    }

    void ProcessExited(object sender, System.EventArgs e)
    {
        string output = System.String.Join("\n", avrdude_output.ToArray());
        Debug.Log(output);
        Debug.Log("Done!");
        avrdude_output.Clear();
    }

    public string upload_program()
    {
        avrdude_output.Clear();
        memtype = "flash:w:\"" + hexfile + "\":i";
        Debug.Log("flashing firmare to microprocessor...");

        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = "\"" + avrdude_path + "\"";
        p.StartInfo.Arguments = "-C \"" + conf_path + "\" -v -p " + processor + " -c " + programmer + " -P " + com_port + " -b " + baud + " -D -U " + memtype;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(DataReceived);
        p.EnableRaisingEvents = true;
        p.Exited += new System.EventHandler(ProcessExited);
        p.Start();
        p.BeginErrorReadLine();
        p.WaitForExit();

        return "cool";
    } 


}
