using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class EyeGazeLSLOutlet : MonoBehaviour
{
    // ストリームの基本情報
    public string StreamName = "UnityEyeGaze";
    public string StreamType = "Gaze";

    private float[] sample_g;   // 送るデータ配列（チャンネル数ぶん作る）
    //private string[] sample_m = new string[1];
    private string[] sample_e = new string[1];

    [System.Serializable]
    public class GazeTarget
    {
        public string targetName;    // マーカーとして送る名前
        public Vector2 centerPos;    // 5m先の平面上での中心点 (x, y)
        public float radiusX;        // ターゲット固有の横半径
        public float radiusY;        // ターゲット固有の縦半径

        [HideInInspector] public float gazeTimer = 0f;
        [HideInInspector] public bool isTriggered = false;
        [HideInInspector] public bool isInside = false;
        [HideInInspector] public int eventLevel;
    }

    [Header("Target Definitions")]
    public List<GazeTarget> targets = new List<GazeTarget>();

    public float[] gazeTimer = new float[3];
    int[] eventLevel = new int[3];
    public bool[] isTriggered = new bool[3];
    public string[] targetName = new string[3];

    private Vector3 lastGaze;


    //タイムスタンプ
    IEnumerator SendMarkers()
    {
        string[] sample_m = new string[1];

        // --- ① 再生開始の LSL 時刻 ---
        double t_start = LSL.LSL.local_clock();
        sample_m[0] = "t_start";
        LSLManager.instance.markerOutlet.push_sample(sample_m, t_start);
        
        Debug.Log($"Audio start at LSL time = {t_start}");

        // --- ② 再生開始から60秒後にもう1度 LS 時刻を送信 ---
        yield return new WaitForSeconds(finishScene_2.instance.playTime);

        double t_finish = LSL.LSL.local_clock();
        sample_m[0] = "t_finish";
        LSLManager.instance.markerOutlet.push_sample(sample_m, t_finish);

        Debug.Log($"Audio finish at LSL time = {t_finish}");
    }

    void SendEventMarker(string name)
    {
        sample_e[0] = "Focus_" + name;
        LSLManager.instance.eventOutlet.push_sample(sample_e);
        Debug.Log($"[LSL Sent] {sample_e[0]} at {Time.time}");
    }

    void SendEventMarker_2(string name)
    {
        sample_e[0] = "Focus_" + name;
        LSLManager.instance.eventOutlet_2.push_sample(sample_e);
        Debug.Log($"[LSL Sent] {sample_e[0]} at {Time.time}");
    }

    void Start()
    {
        sample_g = new float[2];
        //StartCoroutine(SendMarkers());
        lastGaze = Vector3.zero;
    }

    void Update()
    {
        Vector3 gaze = main_GazeHaptics.instance.hitPos;

        if(lastGaze == Vector3.zero && gaze != Vector3.zero && main_GazeHaptics.instance.isPlaying == false)
        {
            StartCoroutine(SendMarkers());
        }

        sample_g[0] = gaze.x;
        sample_g[1] = gaze.y;

        // LSL に送信
        LSLManager.instance.gazeOutlet.push_sample(sample_g);


        float[] distances = new float[3];
        for (int i = 0; i < 3; i++)
        {
            distances[i] = Vector3.Distance(main_GazeHaptics.instance.hapticPoints[i], gaze);
        }
        int minIndex = 0;   // 最小値を探す
        for (int i = 1; i < 3; i++)
        {
            if (distances[i] < distances[minIndex])
            {
                minIndex = i;
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (minIndex == i)
            {
                /*// 範囲内：タイマー加算
                gazeTimer[i] += Time.deltaTime;

                if (gazeTimer[i] >= 0.5f && !isTriggered[i])
                {
                    SendEventMarker(targetName[i]);
                    isTriggered[i] = true;
                }*/

                // 第1段階：0.5秒経過
                if (gazeTimer[i] >= 0.5f && eventLevel[i] < 1)
                {
                    SendEventMarker(targetName[i] + "_0.5s");
                    eventLevel[i] = 1; // 1段階目完了
                }

                // 第2段階：1.0秒経過
                if (gazeTimer[i] >= 1.0f && eventLevel[i] < 2)
                {
                    SendEventMarker(targetName[i] + "_1.0s");
                    eventLevel[i] = 2; // 2段階目完了
                }
            }
            else
            {
                // 範囲外：即座にリセット
                gazeTimer[i] = 0f;
                isTriggered[i] = false;
            }
        }

        lastGaze = gaze;

        foreach (var target in targets)
        {
            // 楕円判定式
            float dx = gaze.x - target.centerPos.x;
            float dy = gaze.y - target.centerPos.y;
            float rx = Mathf.Max(target.radiusX, 0.0001f);
            float ry = Mathf.Max(target.radiusY, 0.0001f);

            float ellipseEquation = (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry);

            if (ellipseEquation <= 1.0f)
            {
                // --- 1. 範囲内に入った瞬間 (Enter) の判定 ---
                if (!target.isInside)
                {
                    SendEventMarker_2(target.targetName + "_Enter");
                    target.isInside = true;
                }

                // --- 2. 滞在時間 (Dwell) の判定 ---
                target.gazeTimer += Time.deltaTime;

                // 0.5秒到達
                if (target.gazeTimer >= 0.5f && target.eventLevel < 1)
                {
                    SendEventMarker_2(target.targetName + "_0.5s");
                    target.eventLevel = 1;
                }

                // 1.0秒到達
                if (target.gazeTimer >= 1.0f && target.eventLevel < 2)
                {
                    SendEventMarker_2(target.targetName + "_1.0s");
                    target.eventLevel = 2;
                }
            }
            else
            {
                // --- 3. 範囲外に出たときのリセット処理 ---
                if (target.isInside)
                {
                    SendEventMarker_2(target.targetName + "_Exit");
                    target.isInside = false;
                }

                target.gazeTimer = 0f;
                target.eventLevel = 0; // 進行度リセット
            }
        }

        /*foreach (var target in targets)
        {
            // 楕円判定式: (Δx^2 / rx^2) + (Δy^2 / ry^2) <= 1
            float dx = gaze.x - target.centerPos.x;
            float dy = gaze.y - target.centerPos.y;

            // ゼロ除算防止
            float rx = Mathf.Max(target.radiusX, 0.0001f);
            float ry = Mathf.Max(target.radiusY, 0.0001f);

            float ellipseEquation = (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry);

            if (ellipseEquation <= 1.0f)
            {
                // 範囲内：タイマー加算
                target.gazeTimer += Time.deltaTime;

                if (target.gazeTimer >= 1.0f && !target.isTriggered)
                {
                    SendEventMarker(target.targetName);
                    target.isTriggered = true;
                }
            }
            else
            {
                // 範囲外：即座にリセット
                target.gazeTimer = 0f;
                target.isTriggered = false;
            }
        }*/
    }
}
