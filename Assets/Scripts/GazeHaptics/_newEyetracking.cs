using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Haptics;
using System;

public class _newEyeTracking : MonoBehaviour
{
    OVREyeGaze eyeGaze;

    public Camera Camera;

    public GameObject screen;

    public bool hitBool { get; private set; }

    //�Đ������ǂ���
    public bool isPlaying = false;

    ///LineRenderer��p����
    LineRenderer linerend;

    //haptic
    public HapticClip clip1;
    public HapticClip clip2;

    HapticClipPlayer _player1;
    HapticClipPlayer _player2;

    //audio
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    public AudioClip audioClip1;
    public AudioClip audioClip2;

    public Vector3 hitPos;

    //�U�����̈ʒu
    private Vector3 hapticPoint1 = new Vector3(-1.5f, 0.5f, 5.0f);
    private Vector3 hapticPoint2 = new Vector3(1.5f, 1.3f, 5.0f);

    private float startTime;

    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        isPlaying = false;

        _player1 = new HapticClipPlayer(clip1);
        _player2 = new HapticClipPlayer(clip2);

        _player1.isLooping = true;
        _player2.isLooping = true;

        _player1.priority = 1;
        _player2.priority = 0;

    }

    //�R���g���[���[�̃{�^����������Haptic���Đ��A��~
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

                    audioSource1.Play();
                    audioSource2.Play();
                }
                else
                {
                    isPlaying = false;

                    _player1.Stop();
                    _player2.Stop();

                    audioSource1.Stop();
                    audioSource2.Stop();
                }
            }
        }

        // If any exceptions occur, we catch and log them here.
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void AdjustHapticAmplitude(float dis, HapticClipPlayer _player)
    {
        if (_player1 != null && _player2 != null)
        {
            float amp = Mathf.Exp(-dis);
            _player.amplitude = amp; // �Q�C���̕ύX
        }
    }


    void Update()
    {
        HandleControllerInput(OVRInput.Controller.RTouch);

        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // �A�C�g���b�L���O�̗L����
        if (eyeGaze.EyeTrackingEnabled)
        {
            // �����̓���
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 25.0f))
            {
                hitBool = true;
                hitPos = hit.point;

                float dis1 = Vector3.Distance(hapticPoint1, hitPos);
                float dis2 = Vector3.Distance(hapticPoint2, hitPos);

                if (dis1 < dis2)
                {
                    _player1.priority = 0;
                    _player2.priority = 1;
                    AdjustHapticAmplitude(dis1, _player1);
                }
                else
                {
                    _player1.priority = 1;
                    _player2.priority = 0;
                    AdjustHapticAmplitude(dis2, _player2);
                }

                //Debug.Log("Should feel vibration.");
            }
            else
            {
                hitBool = false;

                if (_player1 != null)
                {
                    _player1.amplitude = 0.0f; // �Q�C���̕ύX
                }
                if (_player2 != null)
                {
                    _player2.amplitude = 0.0f; // �Q�C���̕ύX
                }
            }

            //Debug.Log("amp:" + _player1.amplitude);
            //Debug.Log(hitPos);
            //Debug.Log(hitBool);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.red);

            //LineRenderer�R���|�[�l���g�̎擾
            linerend = this.GetComponent<LineRenderer>();

            //���̑�����ݒ�
            linerend.startWidth = 0.04f;
            linerend.endWidth = 0.04f;

            //�n�_, �I�_��ݒ肵, �`��
            linerend.SetPosition(0, ray.origin);
            linerend.SetPosition(1, ray.origin + ray.direction * 15);
        }
    }

    protected virtual void OnDestroy()
    {
        _player1?.Dispose();
        _player2?.Dispose();
    }

    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}
