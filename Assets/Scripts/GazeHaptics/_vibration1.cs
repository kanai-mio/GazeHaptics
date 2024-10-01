using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _vibration1 : MonoBehaviour
{
    private float amplitude = 0.05f;
    private float frequency = 15.0f;
    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (_EyeTracking.instance.moving1 && _EyeTracking.instance.isPlaying)
        {
            float vibration = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = new Vector3(initialPosition.x, initialPosition.y + vibration, initialPosition.z);
        }
        else if (initialPosition != transform.position)
        {
            transform.position = initialPosition;
        }
    }
}
