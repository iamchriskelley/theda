using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
//using System.IO.Compression.FileSystem;

public class GerberManager : MonoBehaviour
{
    string GERB_DIR = "/gerbers";
    string SAND_SUBDIR = "/gerbers/sandbox";

    GlobalsManager gm;
    string gnl;
    string timestamp;
    string gerber_header, gerber_data, gerber_comment_theda, gerber_body, gerber_footer;
    
    Dictionary<string, string> g = new Dictionary<string, string>();

    int coord_unit_places = 2, coord_decimal_places = 5;

    // Use this for initialization
    void Start()
    {
        timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        gnl = "\n";
        gm = GetComponent<GlobalsManager>();
        gm.GRID_UNITS = "in";

        g["start"] = "G75*" + gnl;
        g["un_in"] = "%MOIN*%" + gnl;
        g["un_mm"] = "%MOMM*%" + gnl;
        g["offset"] = "%OFA0B0*%" + gnl;
        g["precision"] = "%FSLAX25Y25*%" + gnl;
        g["img_pol"] = "%IPPOS*%" + gnl;
        g["lyr_pol"] = "%LPD*%" + gnl;
        g["octagon_correction"] = "%AMOC8* 5,1,8,0,0,1.08239X$1,22.5 *%" + gnl;

        g["comment_start"] = "G04 ";
        g["comment_end"] = "*"+ gnl;

        g["end"] = "M02 *" + gnl;

        //===========================
        gerber_header = "";
        build_gerber_header(ref gerber_header);
        build_gerber_comment_theda(ref gerber_comment_theda);
        build_gerber_empty_body(ref gerber_body);
        build_gerber_footer(ref gerber_footer);
        Debug.Log("Working in persistent data path: " + Application.persistentDataPath);

        string[] lines = { gerber_header, gerber_comment_theda, gerber_body, gerber_footer};
        
        //write identical empty files
        write_gerber_file("bottomlayer.ger", lines);
        write_gerber_file("bottomsoldermask.ger", lines);

        //write drill file if needed

        //write top copper layer
        lines[2] =
            "%ADD10R,0.03937X0.04331*%\n" +
            "D10*\n";

        List<string[]> topcopper_coords = new List<string[]>();
        topcopper_coords.Add(new string[] { "0.03", "0.03" });
        topcopper_coords.Add(new string[] { "0.1", "0.4" });
        topcopper_coords.Add(new string[] { "0.2", "0.3" });
        topcopper_coords.Add(new string[] { "0.3", "0.2" });
        topcopper_coords.Add(new string[] { "0.4", "0.1" });
        topcopper_coords.Add(new string[] { "0.47", "0.47" });
        for (int i = 0; i < topcopper_coords.Count; i++)
        {
            string[] coord = topcopper_coords[i];
            string coord_line = "";
            build_xy_gerber_string(coord_unit_places, coord_decimal_places, ref coord, ref coord_line);
            coord_line += "D03";
            coord_line += "*\n";
            lines[2] += coord_line;
        }
        write_gerber_file("toplayer.ger", lines);


        //write top solder mask
        lines[2] =
            "%ADD10R,0.04737X0.05131*%\n" +
            "D10*\n";
        List<string[]> topsolder_coords = new List<string[]>();
        topsolder_coords.Add(new string[] { "0.03", "0.03" });
        topsolder_coords.Add(new string[] { "0.1", "0.4" });
        topsolder_coords.Add(new string[] { "0.2", "0.3" });
        topsolder_coords.Add(new string[] { "0.3", "0.2" });
        topsolder_coords.Add(new string[] { "0.4", "0.1" });
        topsolder_coords.Add(new string[] { "0.47", "0.47" });
        for (int i = 0; i < topsolder_coords.Count; i++)
        {
            string[] coord = topsolder_coords[i];
            string coord_line = "";
            build_xy_gerber_string(coord_unit_places, coord_decimal_places, ref coord, ref coord_line);
            coord_line += "D03";
            coord_line += "*\n";
            lines[2] += coord_line;
        }

        //"X0026000Y0022654D03*\n" +
        //"X0026000Y0029346D03*";
        write_gerber_file("topsoldermask.ger", lines);



        //write board outline
        List<string[]> border_coords = new List<string[]>();
        border_coords.Add(new string[] { "0.0", "0.0" });
        border_coords.Add(new string[] { "0.0", "0.5" });
        border_coords.Add(new string[] { "0.5", "0.5" });
        border_coords.Add(new string[] { "0.5", "0.0" });
        
        if(border_coords[0] != border_coords[border_coords.Count - 1]){border_coords.Add(border_coords[0]);}

        lines[2] =
            "%ADD10C,0.00000*%\n" +
            "D10*\n";

        bool first_coord = true;
        for(int i = 0; i < border_coords.Count; i++)
        {
            string[] coord = border_coords[i];
            string coord_line = "";
            build_xy_gerber_string(coord_unit_places, coord_decimal_places, ref coord, ref coord_line);
            
            if (first_coord)
            {
                coord_line += "D02";
                coord_line += "*\n";
                first_coord = false;
            }
            else
            {
                coord_line += "D01";
                coord_line += "*\n";
            }

            lines[2] += coord_line;
        }

        write_gerber_file("board_outline.ger", lines);

        //zip up results and label with generic prefix and timestamp suffix
        string startPath = @"" + Application.persistentDataPath + SAND_SUBDIR;
        string zipPath = @"" + Application.persistentDataPath + GERB_DIR + "/theda_exp_cam_" + timestamp + ".zip";
        string extractPath = @"" + Application.persistentDataPath + GERB_DIR + "/zip_extract_test";

        ZipFile.CreateFromDirectory(startPath, zipPath);
        //ZipFile.ExtractToDirectory(zipPath, extractPath);
    }

    void build_xy_gerber_string(int coord_unit_places, int coord_decimal_places, ref string[] coord, ref string coord_line)
    {
        string[] x_coord, y_coord;
        x_coord = coord[0].Split('.');
        if (x_coord[0].Length > coord_unit_places)
        {
            Debug.Log("Error -- units value too large...truncating...");
            x_coord[0] = x_coord[0].Substring(Math.Max(0, x_coord[0].Length - coord_unit_places));
        }
        if (x_coord[1].Length > coord_decimal_places)
        {
            Debug.Log("Warning -- decimal value too large...truncating ");
            x_coord[1] = x_coord[1].Substring(0, coord_decimal_places);
        }


        y_coord = coord[1].Split('.');
        if (y_coord[0].Length > coord_unit_places)
        {
            Debug.Log("Error -- units value too large...truncating...");
            y_coord[0] = y_coord[0].Substring(Math.Max(0, y_coord[0].Length - coord_unit_places));
        }
        if (y_coord[1].Length > coord_decimal_places)
        {
            Debug.Log("Warning -- decimal value too large...truncating...");
            y_coord[1] = y_coord[1].Substring(0, coord_decimal_places);
        }

        coord_line = "X";
        coord_line += new String('0', Math.Max(0, coord_unit_places - x_coord[0].Length));
        coord_line += x_coord[0];
        coord_line += x_coord[1];
        coord_line += new String('0', Math.Max(0, coord_decimal_places - x_coord[1].Length));

        coord_line += "Y";
        coord_line += new String('0', Math.Max(0, coord_unit_places - y_coord[0].Length));
        coord_line += y_coord[0];
        coord_line += y_coord[1];
        coord_line += new String('0', Math.Max(0, coord_decimal_places - y_coord[1].Length));
    }


    void write_gerber_file(string file_name, string[] lines)
    {
        string full_file_path = Application.persistentDataPath + SAND_SUBDIR + "/" + file_name;
        System.IO.File.WriteAllLines(@full_file_path, lines);
    }

    void build_gerber_header(ref string gerber_header)
    {
        gerber_header = g["start"];
        if (gm.GRID_UNITS == "in") gerber_header += g["un_in"];
        else gerber_header += g["un_mm"];
        gerber_header += g["offset"];
        gerber_header += g["precision"];
        gerber_header += g["img_pol"];
        gerber_header += g["lyr_pol"];
        gerber_header += g["octagon_correction"];
    }

    void build_gerber_comment_theda(ref string gerber_comment)
    {
        gerber_comment = g["comment_start"] + "//==================================" + g["comment_end"];
        gerber_comment += g["comment_start"];
        gerber_comment += "Theda Gerber Builder: Experimental v0.0.1; ";
        gerber_comment += "built " + timestamp;
        gerber_comment += g["comment_end"];
        gerber_comment += g["comment_start"] + "//==================================" + g["comment_end"];
    }

    void build_gerber_empty_body(ref string gerber_body)
    {
        gerber_body = g["comment_start"] + "//  COMMANDS GO HERE  //" + g["comment_end"];
    }

    void build_gerber_footer(ref string gerber_footer)
    {
        gerber_footer = g["end"];
    }
}
