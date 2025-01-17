using UnityEngine;

public class abatar_vibration : MonoBehaviour
{
    //object viibration
    public int objectNum;
    public Transform[] targetObjects;
    public float scaleIntensity = 1.0f; // �X�P�[���̕ω��̋���
    public Vector3[] baseScales;

    //haptic
    public AudioSource[] audioSources;
    private float[] samples = new float[256];

    void Start()
    {
        baseScales = new Vector3[objectNum];

        for (int i = 0; i < objectNum; i++)
        {
            if (targetObjects[i] == null)
            {
                targetObjects[i] = transform;
            }
            baseScales[i] = targetObjects[i].localScale;
        }


    }

    void Update()
    {
        for (int i = 0; i < objectNum; i++)
        {
            if (audioSources[i].isPlaying)
            {
                // �I�[�f�B�I�T���v�����擾
                audioSources[i].GetSpectrumData(samples, 0, FFTWindow.Blackman);

                // �T���v���̍ő�l���擾
                float maxSample = 0f;
                foreach (var sample in samples)
                {
                    if (sample > maxSample)
                    {
                        maxSample = sample;
                    }
                }

                // �X�P�[�����v�Z
                float scaleFactor = 1.0f + (maxSample * scaleIntensity);
                targetObjects[i].localScale = baseScales[i] * scaleFactor;
            }
            else
            {
                // �I�u�W�F�N�g�̃X�P�[�������ɖ߂�
                targetObjects[i].localScale = baseScales[i];
            }
        }


    }
}
