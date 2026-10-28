using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camera;

    private void Start()
    {
        if (Camera.main != null)
        {
            camera = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (camera != null)
        {
            transform.forward = camera.forward;
        }
    }
}
