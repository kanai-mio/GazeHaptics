using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderTransform : MonoBehaviour
{
    public Transform head; // Main Camera (XR Origin内のカメラ) を割り当てる
    public Vector3 offset; // 頭からのオフセット位置

    void Update()
    {
        if (head != null)
        {
            transform.position = head.position + head.rotation * offset;
            transform.rotation = head.rotation;
        }
    }
}
