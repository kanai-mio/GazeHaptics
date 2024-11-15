using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eyeTracking : MonoBehaviour
{
    OVREyeGaze eyeGaze;

    public Camera Camera;

    public GameObject shape1;
    public GameObject shape2;

    private GameObject _hitObject;
    public GameObject HitObject { get => _hitObject; private set => _hitObject = value; }

    //public RaycastHit hitInfo { get; private set; }

    public bool hitBool1 { get; private set; }
    public bool hitBool2 { get; private set; }

    private GameObject _previousHitWindow;

    public static eyeTracking instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public Vector3 HitPoint;

    ///LineRenderer��p����
    LineRenderer linerend;

    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool1 = false;
        hitBool2 = false;
    }

    // �t���[���X�V���ɌĂ΂��
    void Update()
    {
        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // �A�C�g���b�L���O�̗L����
        if (eyeGaze.EyeTrackingEnabled)
        {
            // �����̓���
            //arrow.transform.rotation = eyeGaze.transform.rotation;
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            //shape.transform.position = direction * 3.0f;
            Ray ray = new Ray(Camera.transform.position, direction);
            RaycastHit hit;

            Debug.Log("OK");

            var isHit = Physics.Raycast(ray, out hit, 25.0f);

            if (isHit)
            {
                if (hit.collider.gameObject == shape1)
                {
                    hitBool1 = true;                  
                }
                else if (hit.collider.gameObject == shape2)
                {
                    hitBool2 = true;
                }

                HitPoint = hit.point;
            }
            else
            {
                hitBool1 = false;
                hitBool2 = false;
            }

            //Debug.Log(eyeGaze.transform.rotation);
            //Debug.Log(Camera.transform.position);
            //Debug.Log(HitPoint);
            Debug.Log(hitBool1);
            Debug.Log(hitBool2);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.red);

            //LineRenderer�R���|�[�l���g�̎擾
            linerend = this.GetComponent<LineRenderer>();

            //���̑�����ݒ�
            linerend.startWidth = 0.04f;
            linerend.endWidth = 0.04f;

            //�n�_, �I�_��ݒ肵, �`��
            linerend.SetPosition(0, ray.origin);
            linerend.SetPosition(1, ray.origin + ray.direction * 10);
        }
    }
}
