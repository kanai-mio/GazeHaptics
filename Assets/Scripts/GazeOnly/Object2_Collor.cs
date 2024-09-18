using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object2_Collor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {

        if (eyeTracking.instance.hitBool2)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
