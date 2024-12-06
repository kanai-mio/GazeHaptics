using UnityEngine;
using System;

public class abatar_gazehaptics : MonoBehaviour
{
    OVREyeGaze eyeGaze;
    public Camera Camera;

    //��������
    public bool hitBool { get; private set; }

    //LineRenderer��p����
    LineRenderer linerend;

    //�Đ������ǂ���
    public bool isPlaying = false;

    //audio
    public AudioSource[] audioSources;

    //haptic
    public AudioSource[] hapticSources;
    private float[] samples = new float[256];

    //gaze point
    public Vector3 hitPos;

    //haptic source position
    Vector3[] hapticPoints = new Vector3[]
    {
        new Vector3( 1.0f, 1.3f, 5.0f),
        new Vector3(-1.0f, 1.3f, 5.0f),
        new Vector3( 3.0f, 1.3f, 5.0f),
        new Vector3(-3.0f, 1.3f, 5.0f)
    };

    //�U���̌X��
    public float a;

    //�R���g���[���[�̑���
    //A�ōĐ��A��~
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

    //���_���W�̎擾
    bool IntersectRayWithPlane(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 hitPos)
    {
        hitPos = Vector3.zero;

        // ���C�����ʂƕ��s���ǂ������m�F
        if (Mathf.Approximately(rayDirection.z, 0))
        {
            return false; // ���s�Ō������Ȃ�
        }

        // t ���v�Z
        float t = (5.0f - rayOrigin.z) / rayDirection.z;

        // t �����̏ꍇ�̂݌�_���v�Z�i���C�̑O���̂݁j
        if (t >= 0)
        {
            hitPos = rayOrigin + t * rayDirection;
            return true;
        }

        return false; // ���C�����ʂ̌���Ɍ����Ă���
    }

    //�U���̌v�Z
    void AdjustHapticAmplitude1(float[] distances)
    {
        for (int i = 0; i < 4; i++)
        {
            float vol = Mathf.Exp(-a * distances[i]);
            hapticSources[i].volume = vol;
            Debug.Log("vol" + ": " + vol);
            Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }

    void AdjustHapticAmplitude2(float[] distances)
    {
        int minIndex = 0;   // �ŏ��l��T��
        for (int i = 1; i < distances.Length; i++)
        {
            if (distances[i] < distances[minIndex])
            {
                minIndex = i;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if(i == minIndex)
            {
                hapticSources[i].volume = Mathf.Exp(-a * distances[i]);
            }
            else
            {
                hapticSources[i].volume = 0;
            }
            Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        eyeGaze = GetComponent<OVREyeGaze>();
        hitBool = false;
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        //HandleControllerInput(OVRInput.Controller.RTouch);

        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // �A�C�g���b�L���O�̗L����
        if (eyeGaze.EyeTrackingEnabled)
        {
            // �����̓���
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            //RaycastHit hit;

            //���ʂƂ̌�������
            if (IntersectRayWithPlane(ray.origin, ray.direction, out hitPos))
            {
                float[] distances = new float[4];
                for (int i = 0; i < 4; i++)
                {
                    distances[i] = Vector3.Distance(hapticPoints[i], hitPos);
                    Debug.Log("dis" + i + ": " + distances[i]);
                }

                Debug.Log("hitPos: " + hitPos);

                AdjustHapticAmplitude1(distances);
                //AdjustHapticAmplitude2(distances);
            }
            else
            {
                Debug.Log("No Intersection (ray is parallel to the plane)");
            }

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
}
