using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using LSL;

public class main_GazeHaptics : MonoBehaviour
{
    public static main_GazeHaptics instance;

    OVREyeGaze eyeGaze;
    public Camera Camera;

    public Transform Head;

    private StreamOutlet outlet;
    private string[] sample = new string[1];


    //交差判定
    //public bool hitBool { get; private set; }

    //視線の先に配置するオブジェクト
    public GameObject shape;

    //再生中かどうか
    public bool isPlaying = false;
    //public bool pastBool = false;

    public int termNo;

    //audio
    public AudioSource[] audioSources;

    //haptic
    public AudioSource[] hapticSources;

    //haptic source position
    public Vector3[] hapticPoints;

    public int objectNum;

    //gaze point
    public Vector3 hitPos;

    //視点座標の取得
    bool IntersectRayWithPlane(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 hitPos)
    {
        hitPos = Vector3.zero;

        // レイが平面と平行かどうかを確認
        if (Mathf.Approximately(rayDirection.z, 0))
        {
            return false; // 平行で交差しない
        }

        // t を計算
        float t = (5.0f - rayOrigin.z) / rayDirection.z;

        // t が正の場合のみ交点を計算（レイの前方のみ）
        if (t >= 0)
        {
            hitPos = rayOrigin + t * rayDirection;
            return true;
        }

        return false; // レイが平面の後方に向いている
    }

    //振幅の計算
    void AdjustHapticAmplitude1(float[] distances)
    {
        for (int i = 0; i < objectNum; i++)
        {
            //float vol = Mathf.Exp(-distances[i]);
            float vol = 1 / (1 + distances[i] * distances[i]);
            hapticSources[i].volume = vol ;
            //Debug.Log("vol" + ": " + vol);
            //Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }
    void AdjustHapticAmplitude2(float[] distances)
    {
        int minIndex = 0;   // 最小値を探す
        for (int i = 1; i < objectNum; i++)
        {
            if (distances[i] < distances[minIndex])
            {
                minIndex = i;
            }
        }
        for (int i = 0; i < objectNum; i++)
        {
            if (i == minIndex)
            {
                hapticSources[i].volume = 1;
            }
            else
            {
                hapticSources[i].volume = 0;
            }
            Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }

    void AdjustHapticAmplitude(float[] distances)
    {
        if (termNo == 1)
        {
            AdjustHapticAmplitude1(distances);
        }
        else if (termNo == 2)
        {
            AdjustHapticAmplitude2(distances);
        }
        else if (termNo == 3)
        {
            for (int i = 0; i < objectNum; i++)
            {
                hapticSources[i].volume = 0.4f;
            }
        }
    }

    //タイムスタンプ
    IEnumerator PlayAfterDelay()
    {
        // 5秒待って再生
        yield return new WaitForSeconds(5f);

        // --- ① 再生開始の LSL 時刻 ---
        double t_start = LSL.LSL.local_clock();
        sample[0] = "t_start";
        outlet.push_sample(sample, t_start);

        // AudioSource 再生
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Play();
        }
        foreach (AudioSource hapticSource in hapticSources)
        {
            hapticSource.Play();
        }

        Debug.Log($"Audio start at LSL time = {t_start}");

        // --- ② 再生開始から60秒後にもう1度 LS 時刻を送信 ---
        yield return new WaitForSeconds(60f);

        double t_finish = LSL.LSL.local_clock();
        sample[0] = "t_finish";
        outlet.push_sample(sample, t_finish);

        Debug.Log($"60 sec after start → LSL time = {t_finish}");
    }

    // Start is called before the first frame update
    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        //hitBool = false;
        isPlaying = false;

        StreamInfo streamInfo = new StreamInfo(
            "AudioTrigger",
            "Markers",
            1,
            LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_double64,
            System.Guid.NewGuid().ToString()
        );

        outlet = new StreamOutlet(streamInfo);

        StartCoroutine(PlayAfterDelay());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // アイトラッキングの有効時
        if (eyeGaze.EyeTrackingEnabled)
        {
            // 視線の同期
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            //RaycastHit hit;
            shape.transform.position = Camera.transform.position + direction * 3.0f;

            hitPos = Camera.transform.position + direction * 5.0f;

            //平面との交差判定
            if (IntersectRayWithPlane(ray.origin, ray.direction, out hitPos))
            {
                float[] distances = new float[objectNum];
                for (int i = 0; i < objectNum; i++)
                {
                    distances[i] = Vector3.Distance(hapticPoints[i], hitPos);
                    Debug.Log("dis" + i + ": " + distances[i]);
                }

                Debug.Log("hitPos: " + hitPos);

                //shape.transform.position = hitPos;

                AdjustHapticAmplitude(distances);
            }
            else
            {
                for (int i = 0; i < objectNum; i++)
                {
                    hapticSources[i].volume = 0.0f;
                }
            }
        }
    }
}

