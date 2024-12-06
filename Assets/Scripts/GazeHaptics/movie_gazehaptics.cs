using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;


public class gazehaptics : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;

    OVREyeGaze eyeGaze;

    public Camera Camera;

    public GameObject screen;

    public bool hitBool { get; private set; }

    //LineRendererを用いる
    LineRenderer linerend;

    //再生中かどうか
    public bool isPlaying = false;

    //audio
    public AudioSource audioSource;
    public AudioClip audioClip;

    //haptic
    public AudioSource hapticSource1;
    public AudioSource hapticSource2;
    public AudioSource hapticSource3;
    public AudioSource hapticSource4;

    public AudioClip hapticClip1;
    public AudioClip hapticClip2;
    public AudioClip hapticClip3;
    public AudioClip hapticClip4;

    //視点
    public Vector3 hitPos;

    //振動源の位置
    private Vector3 hapticPoint1 = new Vector3( 1.497f, 1.229f, 5.0f);
    private Vector3 hapticPoint2 = new Vector3(-1.206f, 0.393f, 5.0f);
    private Vector3 hapticPoint3 = new Vector3( 0.708f, 1.225f, 5.0f);
    private Vector3 hapticPoint4 = new Vector3(-0.480f, 0.710f, 5.0f);

    //振動の傾斜
    public float a;

    //コントローラーのボタンを押すとAudio&Hapticを再生、停止
    void HandleControllerInput(OVRInput.Controller controller)
    {
        try
        {
            if (OVRInput.GetDown(OVRInput.Button.One, controller))
            {
                if (!isPlaying)
                {
                    isPlaying = true;

                    audioSource.Play();

                    hapticSource1.Play();
                    hapticSource2.Play();
                    hapticSource3.Play();
                    hapticSource4.Play();

                    this.videoPlayer.Play();

                    Debug.Log("play");
                }
                else
                {
                    isPlaying = false;

                    audioSource.Stop();

                    hapticSource1.Stop();
                    hapticSource2.Stop();
                    hapticSource3.Stop();
                    hapticSource4.Stop();

                    this.videoPlayer.Stop();

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

    void AdjustHapticAmplitude(float dis1, float dis2, float dis3, float dis4)
    {
        float vol1 = Mathf.Exp(-a * dis1);
        float vol2 = Mathf.Exp(-a * dis2);
        float vol3 = Mathf.Exp(-a * dis3);
        float vol4 = Mathf.Exp(-a * dis4);

        hapticSource1.volume = vol1;
        hapticSource2.volume = vol2;
        hapticSource3.volume = vol3;
        hapticSource4.volume = vol4;

        Debug.Log("vol1: " + hapticSource1.volume);
        Debug.Log("vol2: " + hapticSource2.volume);
        Debug.Log("vol3: " + hapticSource3.volume);
        Debug.Log("vol4: " + hapticSource4.volume);
    }

    /*void AdjustHapticAmplitude(float dis1, float dis2)
    {
        float vol1;
        float vol2;

        float dis = Mathf.Min(dis1, dis2, dis3, dis4);

        if (dis1 < dis2)
        {
            vol1 = Mathf.Exp(-dis1);
            vol2 = Mathf.Exp(0);
        }
        else
        {
            vol1 = Mathf.Exp(0);
            vol2 = Mathf.Exp(-dis2);
        }

        hapticSource1.volume = vol1;
        hapticSource2.volume = vol2;

        Debug.Log("vol1: " + hapticSource1.volume);
        Debug.Log("vol2: " + hapticSource2.volume);
    }*/


    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        isPlaying = false;

    }

    void Update()
    {
        HandleControllerInput(OVRInput.Controller.RTouch);

        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // アイトラッキングの有効時
        if (eyeGaze.EyeTrackingEnabled)
        {
            // 視線の同期
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 25.0f))
            {
                hitBool = true;
                this.hitPos = hit.point;

                float dis1 = Vector3.Distance(hapticPoint1, hitPos);
                float dis2 = Vector3.Distance(hapticPoint2, hitPos);
                float dis3 = Vector3.Distance(hapticPoint3, hitPos);
                float dis4 = Vector3.Distance(hapticPoint4, hitPos);

                Debug.Log("hitPos: " + hitPos);
                //Debug.Log("dis1; " + dis1);
                //Debug.Log("dis2: " + dis2);

                //AdjustHapticAmplitude(dis1, dis2, dis3, dis4);
            }
            else
            {
                hitBool = false;

               /* hapticSource1.volume = 0;
                hapticSource2.volume = 0;
                hapticSource3.volume = 0;
                hapticSource4.volume = 0;*/
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

