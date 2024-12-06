using UnityEngine;

public class test_vibration : MonoBehaviour
{
    //object viibration
    private int objectNum = 2;
    public Transform[] targetObjects;
    public float scaleIntensity = 1.0f; // スケールの変化の強さ
    public Vector3[] baseScales;

    //haptic
    public AudioSource[] hapticSources;
    private float[] samples = new float[256];

    void Start()
    {
        for(int i = 0; i < objectNum; i++)
        {
            if (targetObjects[i] == null)
            {
                targetObjects[i] = transform;
            }
            baseScales[i] = targetObjects[i].localScale;
        }

        hapticSources[0].volume = 0.0f;
       
        
    }

    void Update()
    {
        for (int i = 0; i < objectNum; i++)
        {
            if (hapticSources[i].isPlaying)
            {
                // オーディオサンプルを取得
                hapticSources[i].GetSpectrumData(samples, 0, FFTWindow.Blackman);

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
