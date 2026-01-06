using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class pushButton2 : MonoBehaviour
{
    private bool pushed;
    private float startTime;

    public Text text;
    public Text header;
    public Text targetText;

    private bool ready = false;

    IEnumerator ChangeText()
    {
        yield return new WaitForSeconds(10f);
        targetText.text = "準備ができたらAボタンを押して\r\n視聴を始めてください";
        ready = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        pushed = false;
        StartCoroutine(ChangeText());
    }

    void Update()
    {
        header.text = $"タスク2 ({IDdata.Times}/6)";

        if (Gamepad.current == null) return;

        IEnumerator ChangeText()
        {
            yield return new WaitForSeconds(10f);
            targetText.text = "終了です\r\nHMDを外してください";
        }

        try
        {
            /*if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                startTime = Time.time;
                Debug.Log("pushed");
                pushed = true;
            }*/

            if (Gamepad.current.buttonEast.wasPressedThisFrame)
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

        if(pushed && ready)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= 3)
            {
                SceneManager.LoadScene($"_scene{IDdata.scene}");
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
