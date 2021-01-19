using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectronicDevice : MonoBehaviour
{
    [SerializeField] protected StoryData data;
    [SerializeField] protected Camera deviceCamera;

    protected string resourcesDataPath = "Data/Error";

    // Start is called before the first frame update
    protected virtual void Start()
    {
        //Get data specific to device 
        data = Resources.Load<StoryData>(resourcesDataPath);
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

    /// <summary>
    /// If Device has camera set it's activity
    /// </summary>
    /// <param name="_active"></param>
    public void SetCameraActive(bool _active)
    { 
        if(deviceCamera != null)
        {
            deviceCamera.enabled = _active;
        }
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

    public virtual void SetDeviceState(string _stateInfo)
    {

    }

}
