using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class demo_gazehaptics : MonoBehaviour
{
    OVREyeGaze eyeGaze;
    public Camera Camera;

    public Transform Head;

    //public GameObject Cylinder;

    //交差判定
    //public bool hitBool { get; private set; }

    //視線の先に配置するオブジェクト
    public GameObject shape;

    //再生中かどうか
    public bool isPlaying = false;
    public bool pastBool = false;

    public int termNo;

    //audio
    public AudioSource[] audioSources;

    //haptic
    public AudioSource[] hapticSources;

    //cylinder
    public Transform[] Cylinders;
    //public float[] testValues;

    //gaze point
    public Vector3 hitPos;
    private GameObject _hitObject;

    //haptic source position
    public Vector3[] hapticPoints;

    public int objectNum;

    LineRenderer linerend;

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

    // Start is called before the first frame update
    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        //hitBool = false;
        isPlaying = false;
        Vector3 headPos = new Vector3(0.0f, 1.0f, 0.0f);
        /*Head.position = headPos;
        Head.rotation = Quaternion.Euler(0f, 90f, 0f); // ワールド回転*/

        /*for (int i=0; i<3; i++)
        {
            Cylinders[i].SetParent(referenceObject, false);
        }*/
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

            //LineRendererコンポーネントの取得
            linerend = this.GetComponent<LineRenderer>();

            //線の太さを設定
            linerend.startWidth = 0.04f;
            linerend.endWidth = 0.04f;

            //始点, 終点を設定し, 描画
            linerend.SetPosition(0, ray.origin);
            linerend.SetPosition(1, ray.direction * 10);
        }

        for (int i = 0; i < 3; i++)
        {
            float value = Mathf.Clamp01(hapticSources[i].volume);
            //float value = Mathf.Clamp01(testValues[i]);

            Vector3 scale = Cylinders[i].localScale;

            scale.y = Mathf.Lerp(0, 1, value) * 0.5f;
            //scale.y = value * 0.5f;
            Cylinders[i].localScale = scale;

            float newHeight = Mathf.Lerp(0, 1, value) * 0.8f;  // これでscale.yが決まる

            //Cylinders[i].localScale = new Vector3(scale.x, newHeight, scale.z);

            // Cylinderはデフォルトで高さ2だから、scale.y = 1なら高さ2になる
            //float actualHeight = newHeight * 2f;

            // 床に置くなら高さの半分だけ上げる
            //Vector3 pos = Cylinders[i].position;
            //pos.y = scale.y + 0.1f;
            //Cylinders[i].position = pos;
            Vector3 localPos = Cylinders[i].localPosition;
            localPos.y = scale.y + 0.1f;
            Cylinders[i].localPosition = localPos;

        }
    }
}

