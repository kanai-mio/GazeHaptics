using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class pushButton0 : MonoBehaviour
{
    private bool pushed;
    private float startTime;

    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        pushed = false;
    }

    void Update()
    {
        try
        {
            /*if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                startTime = Time.time;
                Debug.Log("pushed");
                pushed = true;
            }
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                startTime = Time.time;
                Debug.Log("pushed");
                pushed = true;
            }*/

            if (Input.GetKeyDown(KeyCode.Space))
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

        if (pushed)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= 3)
            {
                SceneManager.LoadScene($"_scene0");
            }
            else if (elapsedTime >= 2)
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

