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

        // --- ① 再生開始の LSL 時刻 ---
        double t_start = LSL.LSL.local_clock();
        sample_m[0] = "t_start";
        LSLManager.instance.markerOutlet.push_sample(sample_m, t_start);

        Debug.Log($"Audio start at LSL time = {t_start}");

        // --- ② 再生開始から60秒後にもう1度 LS 時刻を送信 ---
        yield return new WaitForSeconds(randTime);

        double t_finish = LSL.LSL.local_clock();
        sample_m[0] = "t_finish";
        LSLManager.instance.markerOutlet.push_sample(sample_m, t_finish);

        Debug.Log($"60 sec after start → LSL time = {t_finish}");

        yield return new WaitForSeconds(5.0f);
        targetText.text = "終了です";
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
            targetText.text = "測定中\r\n何も考えず楽にしていてください";
            StartCoroutine(SendMarkers());
            isMeasuring = true;
        }
    }
}
