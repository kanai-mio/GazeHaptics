using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder_Controller: MonoBehaviour
{
    //cylinder
    public Transform[] Cylinders;

    //haptic
    public AudioSource[] hapticSources;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 3; i++)
        {
            float value = Mathf.Clamp01(hapticSources[i].volume);
            //float value = Mathf.Clamp01(testValues[i]);

            Vector3 scale = Cylinders[i].localScale;

            scale.y = Mathf.Lerp(0, 1, value) * 0.5f;
            //scale.y = value * 0.5f;
            Cylinders[i].localScale = scale;

            float newHeight = Mathf.Lerp(0, 1, value) * 0.8f;  // これでscale.yが決まる

            //Cylinders[i].localScale = new Vector3(scale.x, newHeight, scale.z);

            // Cylinderはデフォルトで高さ2だから、scale.y = 1なら高さ2になる
            //float actualHeight = newHeight * 2f;

            // 床に置くなら高さの半分だけ上げる
            //Vector3 pos = Cylinders[i].position;
            //pos.y = scale.y + 0.1f;
            //Cylinders[i].position = pos;
            Vector3 localPos = Cylinders[i].localPosition;
            localPos.y = scale.y + 0.1f;
            Cylinders[i].localPosition = localPos;
        }
    }
}
