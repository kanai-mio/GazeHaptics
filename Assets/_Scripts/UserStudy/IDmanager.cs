using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IDdata
{
    public static int userID;
    public static int Times;
    public static int termNo;
    public static int scene;
    public static bool isVibration;
}


public class IDmanager : MonoBehaviour
{
    public int ID;
    public int Times;
    public int Term;
    public int scene;
    public bool isVibration;

    // Start is called before the first frame update
    void Start()
    {
        IDdata.userID = ID;
        IDdata.Times = Times;
        IDdata.termNo = Term;
        IDdata.scene = scene;
        IDdata.isVibration = isVibration;
    }
}

