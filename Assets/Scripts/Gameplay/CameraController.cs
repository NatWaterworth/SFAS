using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    Camera cam;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        SetCamera(GetComponent<Camera>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetCamera(Camera _camera)
    {
        if (_camera != null)
        {
            cam = _camera;
        }
        else
        {
            Debug.LogError("Camera is null for this CameraController: " + this);
        }
    }

    public void SetCameraActive(bool _active)
    {
        if (cam != null)
        {
            cam.enabled = _active;
        }
        else
        {
            Debug.LogError("Camera is null for this CameraController: " + this);
        }
    }

}
