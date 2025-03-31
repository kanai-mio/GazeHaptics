using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class finishScene_1 : MonoBehaviour
{
    public float startTime;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float pastTime = Time.time - startTime; 
        if(pastTime > 30)
        {
            SceneManager.LoadScene("Questionnaire_1");
        }
    }
}
