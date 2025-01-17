using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class pushButton : MonoBehaviour
{
    private bool pushed;
    private float startTime;

    [SerializeField]
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        pushed = false;
    }

    // Update is called once per frame
    void Update()
    {
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
                SceneManager.LoadScene("Questionnaire");
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
