using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class timeManager : MonoBehaviour
{
    private bool isMeasuring;

    private float randTime;

    public Text targetText;

    IEnumerator SendMarkers()
    {
        string[] sample_m = new string[1];

        // --- start marker ---
        double t_start = LSL.LSL.local_clock();
        sample_m[0] = "t_start";
        LSLManager.instance.markerOutlet.push_sample(sample_m, t_start);

        Debug.Log($"Audio start at LSL time = {t_start}");

        // --- finish marker ---
        yield return new WaitForSeconds(randTime);

        double t_finish = LSL.LSL.local_clock();
        sample_m[0] = "t_finish";
        LSLManager.instance.markerOutlet.push_sample(sample_m, t_finish);

        Debug.Log($"60 sec after start ¨ LSL time = {t_finish}");

        yield return new WaitForSeconds(5.0f);
        targetText.text = "Finished";
    }

    // Start is called before the first frame update
    void Start()
    {
        isMeasuring = false;
        randTime = Random.Range(150.0f, 180.0f);
        Debug.Log("masurementTime: " + randTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isMeasuring == false)
        {
            Debug.Log("recode start");
            targetText.text = "Recording";
            StartCoroutine(SendMarkers());
            isMeasuring = true;
        }
    }
}
