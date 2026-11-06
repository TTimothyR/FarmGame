using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camera;

    private void Start()
    {
        if (Camera.main != null)
        {
            _camera = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (_camera != null)
        {
            transform.forward = _camera.forward;
        }
    }
}
