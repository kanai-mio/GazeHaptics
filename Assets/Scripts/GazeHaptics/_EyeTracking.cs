using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Haptics;
using System;

public class _EyeTracking : MonoBehaviour
{
    OVREyeGaze eyeGaze;

    public Camera Camera;

    public GameObject screen;

    public bool hitBool { get; private set; }

    //再生中かどうか
    public bool isPlaying = false;

    ///LineRendererを用いる
    LineRenderer linerend;

    //haptic
    public HapticClip clip1;
    public HapticClip clip2;
    public HapticClip clip3;

    HapticClipPlayer _player1;
    HapticClipPlayer _player2;
    HapticClipPlayer _player3;

    //audio
    public AudioSource audioSource1;
    //public AudioSource audioSource2;
    //public AudioSource audioSource3;

    public AudioClip audioClip1;
    //public AudioClip audioClip2;
    //public AudioClip audioClip3;

    public Vector3 hitPos;

    //振動源の位置
    private Vector3 hapticPoint1 = new Vector3(-2.5f, 1.2f, 5.0f);
    private Vector3 hapticPoint2 = new Vector3( 0.0f, 0.6f, 5.0f);
    private Vector3 hapticPoint3 = new Vector3( 2.5f, 0.0f, 5.0f);

/*    public bool playing1 { get; private set; }
    public bool playing2 { get; private set; }
    public bool playing3 { get; private set; }*/

    /*public bool moving1 { get; private set; }
    public bool moving2 { get; private set; }
    public bool moving3 { get; private set; }*/

    private float startTime;

    public static _EyeTracking instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        isPlaying = false;

        _player1 = new HapticClipPlayer(clip1);
        _player2 = new HapticClipPlayer(clip2);
        _player3 = new HapticClipPlayer(clip3);

        _player1.isLooping = true;
        _player2.isLooping = true;
        _player3.isLooping = true;

        /*audioSource1 = gameObject.GetComponent<AudioSource>();  // AudioSourceコンポーネントを取得
        audioSource2 = gameObject.GetComponent<AudioSource>();
        audioSource3 = gameObject.GetComponent<AudioSource>();
        audioSource1.clip = audioClip1;
        audioSource2.clip = audioClip2;
        audioSource3.clip = audioClip3;*/

        /* playing1 = false;
         playing2 = false;
         playing3 = false;*/

        /*moving1 = false;
        moving2 = false;
        moving3 = false;*/
    }

    //コントローラーのボタンを押すとHapticを再生、停止
    void HandleControllerInput(OVRInput.Controller controller)
    {
        try
        {
            if (OVRInput.GetDown(OVRInput.Button.One, controller))
            {
                if (!isPlaying)
                {
                    startTime = Time.time;

                    isPlaying = true;
                    //playing1 = true;
                    _player1.Play(Controller.Right);
                    audioSource1.Play();
                }
                else
                {
                    isPlaying = false;

                    _player1.Stop();
                    //_player2.Stop();
                    //_player3.Stop();

                    audioSource1.Stop();
                    //audioSource2.Stop();
                    //audioSource3.Stop();

                    /*moving1 = false;
                    moving2 = false;
                    moving3 = false;*/


                    //Debug.Log("Vibration should stop.");
                }
            }
        }

        // If any exceptions occur, we catch and log them here.
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void AdjustHapticAmplitude1(Vector3 hitPos)
    {
        if (_player1 != null)
        {
            float dis1 = Vector3.Distance(hapticPoint1, hitPos);
            float dis2 = Vector3.Distance(hapticPoint2, hitPos);
            float dis3 = Vector3.Distance(hapticPoint3, hitPos);
            Debug.Log("dis1:" + dis1);
            Debug.Log("dis2:" + dis2);
            Debug.Log("dis3:" + dis3);

            float amp = Mathf.Exp(-dis1) + Mathf.Exp(-dis2) + Mathf.Exp(-dis3);
            _player1.amplitude = amp; // ゲインの変更
        }
    }

    void AdjustHapticAmplitude2 (Vector3 hitPos)
    {
        if (_player1 != null && _player2 != null && _player3 != null)
        {
            float dis1 = Vector3.Distance(hapticPoint1, hitPos);
            float dis2 = Vector3.Distance(hapticPoint2, hitPos);
            float dis3 = Vector3.Distance(hapticPoint3, hitPos);
            
            float dis = Mathf.Min(dis1, dis2, dis3);
            float amp = Mathf.Exp(-dis);

            if(dis == dis1)
            {
                _player1.amplitude = amp;
                _player1.priority = 1;
                _player2.priority = 0;
                _player3.priority = 0;
                Debug.Log("dis1:" + dis1);
            }
            else if(dis == dis2)
            {
                _player2.amplitude = amp;
                _player1.priority = 0;
                _player2.priority = 1;
                _player3.priority = 0;
                Debug.Log("dis2:" + dis2);
            }
            else if (dis == dis3)
            {
                _player3.amplitude = amp;
                _player1.priority = 0;
                _player2.priority = 0;
                _player3.priority = 1;
                Debug.Log("dis3:" + dis3);
            }
        }
    }

    /*void AdjustHapticAmplitude3 (Vector3 hitPos)
    {
        float amp = 0.0f;
        *//*
        if (_player1 != null && moving1)
        {
            float dis = Vector3.Distance(hapticPoint1, hitPos);
            amp = Mathf.Exp(-dis);
            _player1.amplitude = amp;            
        }
        if (_player2 != null && moving2)
        {
            float dis = Vector3.Distance(hapticPoint2, hitPos);
            amp = Mathf.Exp(-dis);
            _player2.amplitude = amp;
        }
        if (_player3 != null && moving3)
        {
            float dis = Vector3.Distance(hapticPoint3, hitPos);
            amp = Mathf.Exp(-dis);
            _player3.amplitude = amp;
        }*//*

        if(_player1 != null)
        {
            if(moving1)
            {
                float dis = Vector3.Distance(hapticPoint1, hitPos);
                amp = Mathf.Exp(-dis);
                _player1.amplitude = amp;
            }
            if (moving2)
            {
                float dis = Vector3.Distance(hapticPoint2, hitPos);
                amp = Mathf.Exp(-dis);
                _player1.amplitude = amp;
            }
            if (moving3)
            {
                float dis = Vector3.Distance(hapticPoint3, hitPos);
                amp = Mathf.Exp(-dis);
                _player1.amplitude = amp;
            }
        }

        Debug.Log("amp: " + amp);
    }*/

    /*void Moving (float nowTime)
    {
        float time = nowTime - startTime;
        Debug.Log(time);

        if (time < 10.0f)
        {
            moving1 = true;
            moving2 = false;
            moving3 = false;
        }
        else if (time < 20.0f)
        {
            moving1 = false;
            moving2 = true;
            moving3 = false;
        }
        else if (time < 30.0f)
        {
            moving1 = false;
            moving2 = false;
            moving3 = true;
        }
        else
        {
            moving1 = false;
            moving2 = false;
            moving3 = false;
        }
    }*/

    // フレーム更新毎に呼ばれる
    void Update()
    {
        HandleControllerInput(OVRInput.Controller.RTouch);

        //Moving(Time.time);
 
        /*if (isPlaying && !moving1 && !moving2 && !moving3)
        {
            isPlaying = false;
            _player1.Stop();
            audioSource1.Stop();
        }*/

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
                AdjustHapticAmplitude2(hitPos);

                //Debug.Log("Should feel vibration.");
            }
            else
            {
                hitBool = false;

                if (_player1 != null)
                {
                    _player1.amplitude = 0.0f; // ゲインの変更
                }
                if (_player2 != null)
                {
                    _player2.amplitude = 0.0f; // ゲインの変更
                }
                if (_player3 != null)
                {
                    _player3.amplitude = 0.0f; // ゲインの変更
                }
            }

            //Debug.Log("amp:" + _player1.amplitude);
            //Debug.Log(hitPos);
            //Debug.Log(hitBool);
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

    protected virtual void OnDestroy()
    {
        _player1?.Dispose();
        _player2?.Dispose();
        _player3?.Dispose();
    }

    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}
