using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class demo2_gazehaptics : MonoBehaviour
{
    OVREyeGaze eyeGaze;
    public Camera Camera;

    //交差判定
    //public bool hitBool { get; private set; }

    //再生中かどうか
    public bool isPlaying = false;
    public bool pastBool = false;

    public int termNo;

    //audio
    public AudioSource[] audioSources;

    //haptic
    public AudioSource[] hapticSources;

    //gaze point
    public Vector3 hitPos;

    //haptic source position
    public Vector3[] hapticPoints;

    public int objectNum;

    /*//振動の傾斜
    public float[] a;

    public TextAsset csvFile; // UnityのInspectorでCSVファイルをアタッチ
*/
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
            float vol = Mathf.Exp(-distances[i]);
            hapticSources[i].volume = vol;
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

    // Start is called before the first frame update
    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        //hitBool = false;
        isPlaying = false;
        Debug.Log($"termNo:{termNo}");

        /*string[] lines = csvFile.text.Split('\n'); // 改行で行を分割
        string[] columns = lines[0].Split(','); // カンマで列を分割
        for (int i = 0; i < objectNum; i++)
        {
            Debug.Log($"columns[{i}]: {columns[i]}");
            if (float.TryParse(columns[i], out float amp))
            {
                a[i] = amp;
            }
        }*/

        double startTime = Time.time + 1.0f;
        foreach (var ausioSource in audioSources)
        {
            ausioSource.PlayScheduled(startTime);
        }
        foreach (var hapticSource in hapticSources)
        {
            hapticSource.PlayScheduled(startTime);
        }
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
            RaycastHit hit;

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

