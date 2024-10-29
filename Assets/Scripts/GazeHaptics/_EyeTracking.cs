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
    //public HapticClip clip3;

    HapticClipPlayer _player1;
    HapticClipPlayer _player2;
    //HapticClipPlayer _player3;

    //audio
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    //public AudioSource audioSource3;

    public AudioClip audioClip1;
    public AudioClip audioClip2;
    //public AudioClip audioClip3;

    public Vector3 hitPos;

    //振動源の位置
    private Vector3 hapticPoint1 = new Vector3(-1.5f, 0.5f, 5.0f);
    private Vector3 hapticPoint2 = new Vector3( 1.5f, 1.3f, 5.0f);
    //private Vector3 hapticPoint3 = new Vector3( 2.5f, 0.6f, 5.0f);

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
        //isPlaying = false;

        _player1 = new HapticClipPlayer(clip1);
        _player2 = new HapticClipPlayer(clip2);
        //_player3 = new HapticClipPlayer(clip3);

        _player1.isLooping = true;
        _player2.isLooping = true;
        //_player3.isLooping = true;

        _player1.Play(Controller.Right);
        //_player2.Play(Controller.Right);
        _player1.priority = 1;
        //_player2.priority = 0;
        //audioSource1.Play();
        //audioSource2.Play();
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
                    _player1.Play(Controller.Right);
                    _player2.Play(Controller.Right);
                    _player1.priority = 1;
                    _player2.priority = 0;
                    audioSource1.Play();
                }
                else
                {
                    isPlaying = false;

                    _player1.Stop();
                    _player2.Stop();
                    //_player3.Stop();

                    audioSource1.Stop();

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
        if (_player1 != null && _player2 != null)
        {
            float dis1 = Vector3.Distance(hapticPoint1, hitPos);
            float dis2 = Vector3.Distance(hapticPoint2, hitPos);
            //float dis3 = Vector3.Distance(hapticPoint3, hitPos);

            Debug.Log("dis1:" + dis1);
            Debug.Log("dis2:" + dis2);
            //Debug.Log("dis3:" + dis3);

            float amp1 = Mathf.Exp(-dis1);
            _player1.amplitude = amp1; // ゲインの変更
            float amp2 = Mathf.Exp(-dis2);
            _player2.amplitude = amp2; // ゲインの変更

            if (dis1 < dis2)
            {
                _player1.priority = 1;
                _player2.priority = 0;
            }
            else
            {
                _player1.priority = 0;
                _player2.priority = 1;
            }
            Debug.Log("p1:" + _player1.priority);
            Debug.Log("p2:" + _player2.priority);
        }
    }

    /*void AdjustHapticAmplitude2 (Vector3 hitPos)
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
    }*/



    // フレーム更新毎に呼ばれる
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
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 25.0f))
            {
                hitBool = true;
                hitPos = hit.point;

                /*float dis1 = Vector3.Distance(hapticPoint1, hitPos);
                float dis2 = Vector3.Distance(hapticPoint2, hitPos);

                if (dis1 < dis2)
                {
                    _player1.priority = 1;
                    _player2.priority = 0;
                }
                else
                {
                    _player1.priority = 0;
                    _player2.priority = 1;
                }*/

                //AdjustHapticAmplitude1(hitPos);

                //Debug.Log("Should feel vibration.");
            }
            else
            {
                hitBool = false;

                /*if (_player1 != null)
                {
                    _player1.amplitude = 0.0f; // ゲインの変更
                }
                if (_player2 != null)
                {
                    _player2.amplitude = 0.0f; // ゲインの変更
                }*/
                /*
                if (_player3 != null)
                {
                    _player3.amplitude = 0.0f; // ゲインの変更
                }*/
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
        //_player3?.Dispose();
    }

    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}
