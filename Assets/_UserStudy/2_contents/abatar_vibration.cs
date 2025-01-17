using UnityEngine;

public class abatar_vibration : MonoBehaviour
{
    //object viibration
    public int objectNum;
    public Transform[] targetObjects;
    public float scaleIntensity = 1.0f; // スケールの変化の強さ
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
                // オーディオサンプルを取得
                audioSources[i].GetSpectrumData(samples, 0, FFTWindow.Blackman);

                // サンプルの最大値を取得
                float maxSample = 0f;
                foreach (var sample in samples)
                {
                    if (sample > maxSample)
                    {
                        maxSample = sample;
                    }
                }

                // スケールを計算
                float scaleFactor = 1.0f + (maxSample * scaleIntensity);
                targetObjects[i].localScale = baseScales[i] * scaleFactor;
            }
            else
            {
                // オブジェクトのスケールを元に戻す
                targetObjects[i].localScale = baseScales[i];
            }
        }


    }
}
