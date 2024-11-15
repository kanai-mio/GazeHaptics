using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Haptics;
using System;
using NAudio.Wave;


public class gaze_haptics : MonoBehaviour
{
    OVREyeGaze eyeGaze;

    public Camera Camera;

    public GameObject screen;

    public bool hitBool { get; private set; }

    //LineRendererを用いる
    LineRenderer linerend;

    //再生中かどうか
    public bool isPlaying = false;

    //audio
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    public AudioClip audioClip1;
    public AudioClip audioClip2;

    //haptic
    public AudioSource hapticSource1;
    public AudioSource hapticSource2;

    public AudioClip hapticClip1;
    public AudioClip hapticClip2;

    //視点
    public Vector3 hitPos;

    //振動源の位置
    private Vector3 hapticPoint1 = new Vector3(-1.5f, 0.5f, 5.0f);
    private Vector3 hapticPoint2 = new Vector3(1.5f, 1.3f, 5.0f);

    /*//振動の傾斜
    private float hap1 = 1.0f;
    private float hap2 = 1.0f;*/

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

                    audioSource1.Play();
                    audioSource2.Play();

                    hapticSource1.Play();
                    hapticSource2.Play();

                    Debug.Log("play");
                }
                else
                {
                    isPlaying = false;

                    audioSource1.Stop();
                    audioSource2.Stop();

                    hapticSource1.Stop();
                    hapticSource2.Stop();

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

    void AdjustHapticAmplitude(float dis1, float dis2)
    {
        float vol1 = Mathf.Exp(-dis1);
        float vol2 = Mathf.Exp(-dis2);

        hapticSource1.volume = vol1;
        hapticSource2.volume = vol2;

        Debug.Log("vol1: " + hapticSource1.volume);
        Debug.Log("vol2: " + hapticSource2.volume);
    }

    /*void AdjustHapticAmplitude(float dis1, float dis2)
    {
        if (dis1 < dis2)
        {
            float vol = Mathf.Exp(-dis1);
            audioDict["acoustic_guitar"].AdjustVolume(hap1 * vol1);
            audioDict["rain"].AdjustVolume(0);
        }
        else
        {
            float vol = Mathf.Exp(-dis2);
            audioDict["acoustic_guitar"].AdjustVolume(0);
            audioDict["rain"].AdjustVolume(hap2 * vol2);
        }
    }*/

    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        isPlaying = false;

    }

    void Update()
    {
        HandleControllerInput(OVRInput.Controller.LTouch);

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
                hitPos = hit.point;

                float dis1 = Vector3.Distance(hapticPoint1, hitPos);
                float dis2 = Vector3.Distance(hapticPoint2, hitPos);

                //Debug.Log("hitPos: " + hitPos);
                //Debug.Log("dis1; " + dis1);
                //Debug.Log("dis2: " + dis2);

                AdjustHapticAmplitude(dis1, dis2);
            }
            else
            {
                hitBool = false;

                //audioDict["acoustic_guitar"].AdjustVolume(0);
                //audioDict["rain"].AdjustVolume(0);
            }

            Debug.DrawRay(ray.origin, ray.direction * 15, Color.red);

            //LineRendererコンポーネントの取得
            linerend = this.GetComponent<LineRenderer>();

            //線の太さを設定
            linerend.startWidth = 0.04f;
            linerend.endWidth = 0.04f;

            //始点, 終点を設定し, 描画
            linerend.SetPosition(0, ray.origin);
            linerend.SetPosition(1, ray.origin + ray.direction * 15);
        }
    }
}
