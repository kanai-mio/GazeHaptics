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

    //�Đ������ǂ���
    private bool isIntersecting = false;

    ///LineRenderer��p����
    LineRenderer linerend;

    //haptic
    public HapticClip clip;

    HapticClipPlayer _playerLeft;
    HapticClipPlayer _playerRight;

    public float highAmplitude = 1.0f; // �������I�u�W�F�N�g�Ɍ����Ă��鎞�̃Q�C��
    public float lowAmplitude = 0.0f;  // �������I�u�W�F�N�g�Ɍ����Ă��Ȃ����̃Q�C��

    public Vector3 hitPos;

    //�U�����̈ʒu
    private Vector3 hapticPoint1 = new Vector3(-0.6f, 0.7f, 2.5f);
    private Vector3 hapticPoint2 = new Vector3( 0.6f, 0.7f, 2.5f);


    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        //_playerLeft = new HapticClipPlayer(clip);
        _playerRight = new HapticClipPlayer(clip);
    }

    //�R���g���[���[�̃{�^����������Haptic���Đ��A��~
    void HandleControllerInput(OVRInput.Controller controller, HapticClipPlayer clipPlayer, Controller hand)
    {
        try
        {
            if (OVRInput.GetDown(OVRInput.Button.One, controller))
            {
                if (!isIntersecting)
                {
                    isIntersecting = true;
                    _playerRight.Play(Controller.Right);
                    //Debug.Log("Start vibration.");
                }
                else
                {
                    isIntersecting = false;
                    _playerRight.Stop();
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

    void AdjustHapticAmplitude1(Vector3 pos1, Vector3 pos2, Vector3 hitPos)
    {
        if (_playerRight != null)
        {
            float dis1 = Vector3.Distance(pos1, hitPos);
            float dis2 = Vector3.Distance(pos2, hitPos);
            Debug.Log("dis1:" + dis1);
            Debug.Log("dis2:" + dis2);

            float amp = Mathf.Exp(-dis1) + Mathf.Exp(-dis2);
            _playerRight.amplitude = amp; // �Q�C���̕ύX
        }
    }

    void AdjustHapticAmplitude2 (Vector3 pos1, Vector3 pos2, Vector3 hitPos)
    {
        if (_playerRight != null)
        {
            float dis1 = Vector3.Distance(pos1, hitPos);
            float dis2 = Vector3.Distance(pos2, hitPos);
            Debug.Log("dis1:" + dis1);
            Debug.Log("dis2:" + dis2);

            float dis = Mathf.Min(dis1, dis2);
            float amp = Mathf.Exp(dis);
            _playerRight.amplitude = amp; // �Q�C���̕ύX
        }
    }

    // �t���[���X�V���ɌĂ΂��
    void Update()
    {
        HandleControllerInput(OVRInput.Controller.RTouch, _playerRight, Controller.Right);

        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // �A�C�g���b�L���O�̗L����
        if (eyeGaze.EyeTrackingEnabled)
        {
            // �����̓���
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 25.0f) && hit.collider.gameObject == screen)
            {
                hitBool = true;
                hitPos = hit.point;
                AdjustHapticAmplitude1(hapticPoint1, hapticPoint2, hitPos);

                //Debug.Log("Should feel vibration.");
            }
            else
            {
                hitBool = false;

                if (_playerRight != null)
                {
                    _playerRight.amplitude = 0.0f; // �Q�C���̕ύX
                }
            }

            Debug.Log("amp:" + _playerRight.amplitude);
            //Debug.Log(hitPos);
            Debug.Log(hitBool);
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
        _playerLeft?.Dispose();
        _playerRight?.Dispose();
    }

    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}
