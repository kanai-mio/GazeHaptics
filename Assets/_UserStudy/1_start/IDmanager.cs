using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class IDdata
{
    public static int userID;
    public static int termNo;
}

public class IDmanager : MonoBehaviour
{
    public int ID;
    public int No;

    // Start is called before the first frame update
    void Start()
    {
        IDdata.userID = ID;
        IDdata.termNo = No;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
/*
[CreateAssetMenu(menuName = "Game Data")]
public class IDdata : ScriptableObject
{
    private static IDmanager manager;

    public int userID = manager.userID;
    public int termNo = manager.termNo;
}*/
