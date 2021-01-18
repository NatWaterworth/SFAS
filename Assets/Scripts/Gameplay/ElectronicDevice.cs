using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectronicDevice : MonoBehaviour
{
    protected StoryData data;
    [SerializeField] Camera deviceCamera;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    public Camera GetDeviceCamera()
    {
        return deviceCamera;
    }

    public bool HasDeviceCamera()
    {
        if (deviceCamera == null)
            return false;
        return true;
    }

    public bool HasDeviceData()
    {
        if (data != null)
            return true;
        return false;
    }

    public StoryData GetDeviceData()
    {
        return data;
    }

}
