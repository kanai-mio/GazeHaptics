using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _vibration : MonoBehaviour
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
        float vibration = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(initialPosition.x, initialPosition.y + vibration, initialPosition.z);
    }
}
