using UnityEngine;
using System;

public class abatar_gazehaptics : MonoBehaviour
{
    OVREyeGaze eyeGaze;
    public Camera Camera;

    //交差判定
    public bool hitBool { get; private set; }

    //LineRendererを用いる
    LineRenderer linerend;

    //再生中かどうか
    public bool isPlaying = false;

    //audio
    public AudioSource[] audioSources;

    //haptic
    public AudioSource[] hapticSources;
    private float[] samples = new float[256];

    //gaze point
    public Vector3 hitPos;

    //haptic source position
    public Vector3[] hapticPoints;

    public int objectNum;
    public float mixGain;

    //振動の傾斜
    public float a;

    //コントローラーの操作
    //Aで再生、停止
    void HandleControllerInput(OVRInput.Controller controller)
    {
        try
        {
            if (OVRInput.GetDown(OVRInput.Button.One, controller))
            {
                if (!isPlaying)
                {
                    isPlaying = true;

                    foreach (var audio in audioSources)
                    {
                        audio.Play();
                    }

                    foreach (var haptic in hapticSources)
                    {
                        haptic.Play();
                    }

                    Debug.Log("play");
                }
                else
                {
                    isPlaying = false;

                    foreach (var audio in audioSources)
                    {
                        audio.Stop();
                    }

                    foreach (var haptic in hapticSources)
                    {
                        haptic.Stop();
                    }

                    Debug.Log("stop");
                }
            }
        }

        // If any exceptions occur, we catch and log them here.
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

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
            float vol = Mathf.Exp(-a * distances[i]);
            hapticSources[i].volume = vol;
            Debug.Log("vol" + ": " + vol);
            Debug.Log("vol" + i + ": " + hapticSources[i].volume);
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
            if(i == minIndex)
            {
                hapticSources[i].volume = 1.0f;
            }
            else
            {
                hapticSources[i].volume = 0;
            }
            Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }

    public int termNo;
    void AdjustHapticAmplitude(float[] distances)
    {
        //termNo = IDdata.termNo;
        if (termNo == 1)
        {
            AdjustHapticAmplitude1(distances);
        }
        else if(termNo == 2)
        {
            AdjustHapticAmplitude2(distances);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        isPlaying = false;
        Debug.Log($"termNo:{termNo}");

        foreach (var haptic in hapticSources)
        {
            haptic.volume = mixGain;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //HandleControllerInput(OVRInput.Controller.RTouch);

        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // アイトラッキングの有効時
        if (eyeGaze.EyeTrackingEnabled)
        {
            // 視線の同期
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            //RaycastHit hit;

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
                Debug.Log("No Intersection (ray is parallel to the plane)");
            }

            /*Debug.DrawRay(ray.origin, ray.direction * 15, Color.red);

            //LineRendererコンポーネントの取得
            linerend = this.GetComponent<LineRenderer>();

            //線の太さを設定
            linerend.startWidth = 0.04f;
            linerend.endWidth = 0.04f;

            //始点, 終点を設定し, 描画
            linerend.SetPosition(0, ray.origin);
            linerend.SetPosition(1, ray.origin + ray.direction * 15);*/
        }
    }
}
