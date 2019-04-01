using UnityEngine;
using System;
using RAS503;

public class HRC : MonoBehaviour {
    public HECRASController hrc;
    void Start () {
        hrc = new HECRASController();
        //--------------------------------------------
        int nmsg = 0;
        bool block = true;
        Array sa = null;
        hrc.Project_Open("C:/Users/admin/Documents/HEC Data/HEC-RAS/Example Projects/1D Steady Flow Hydraulics/Chapter 4 Example Data/Ex1.prj");
        Debug.Log(hrc.Project_Current());
        Debug.Log(hrc.CurrentGeomFile());
        Debug.Log(hrc.CurrentSteadyFile());
        Debug.Log(hrc.CurrentProjectFile());
        Debug.Log(hrc.CurrentPlanFile());
        hrc.Compute_ShowComputationWindow();

        try
        {
            hrc.Compute_CurrentPlan(ref nmsg, ref sa, ref block);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        //hrc = null; //kills the process faster than waiting for GC
        //--------------------------------------------
        //Console.WriteLine("Press any key to quit.");
        //Console.ReadKey();
        hrc.Project_Save();
        hrc.Project_Close();
        hrc.QuitRas();
    }
}
