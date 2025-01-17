using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class finish_Scene : MonoBehaviour
{
    //audio
    public AudioSource[] audioSources;
    //haptic
    public AudioSource[] hapticSources;
    //Ä¶’†‚©‚Ç‚¤‚©
    public bool isPlaying = false;

    public bool pastBool = false;

    public float finishedTime = 100f;

    private bool statusCheck()
    {
        foreach (var audio in audioSources)
        {
            if (audio.isPlaying)
            {
                return true;
            }
        }
        foreach (var haptic in hapticSources)
        {
            if (haptic.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        isPlaying = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlaying)
        {
            pastBool = true;
        }
        else
        {
            pastBool = false;
        }

        isPlaying = statusCheck();

        if(!isPlaying)
        {
            if (pastBool)
            {
                finishedTime = Time.time;
            }
            else if (Time.time - finishedTime > 2.0f)
            {
                SceneManager.LoadScene("Questionnaire");
            }
        }
    }
}
