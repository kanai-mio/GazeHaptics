using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;

public class pushButton2 : MonoBehaviour
{
    private bool pushed;
    private float startTime;

    public Text text;
    public Text header;

    // Start is called before the first frame update
    void Start()
    {
        pushed = false;
        
    }

    void Update()
    {
        header.text = $"ƒ^ƒXƒN2 ({IDdata.Times}/6)";
        try
        {
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                startTime = Time.time;
                Debug.Log("pushed");
                pushed = true;
            }
        }

        // If any exceptions occur, we catch and log them here.
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        if(pushed)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= 3)
            {
                SceneManager.LoadScene($"_scene2-{IDdata.scene}");
            }
            else if(elapsedTime >= 2)
            {
                text.text = "1";
            }
            else if (elapsedTime >= 1)
            {
                text.text = "2";
            }
            else
            {
                text.text = "3";
            }
        }
    }
}
