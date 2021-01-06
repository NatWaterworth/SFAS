using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecuirityCamera : ElectronicDevice
{
    public enum CameraState
    {
        Static,
        SentryMode
    }

    [SerializeField]
    CameraState currentState;
    Camera cam;

    #region Sentry Mode Variables
    [Header("Sentry Mode Variables")]
    [SerializeField] Vector3 leftMostEulerRotation;
    [SerializeField] Vector3 rightMostEulerRotation;
    [SerializeField][Range(0,1)] float rotationSpeed;
    float sentryTick;

    const string resourcesDataPath = "Data/SecuirityCamera";

    #endregion

    #region Player Detection Variables
    PlayerDetector detector;
    [SerializeField] float detectionRadius;
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (GetComponentInChildren<Camera>() != null)
        {
            cam = GetComponentInChildren<Camera>();
        }

        //Get data specific to secuirity camera     
        data = Resources.Load<StoryData>(resourcesDataPath);

        if (data == null)
        {
            Debug.LogError(this + " cannot find data at path: " + resourcesDataPath + ". Need data to change camera state.");
        }

        detector = new PlayerDetector();

        //Set rotation centre
        cam.transform.localEulerAngles = leftMostEulerRotation;
    }

    // Update is called once per frame
    void Update()
    {
        SetStateInformation();

        detector.DetectPlayer(cam.transform, Vector3.zero, detectionRadius);

    }


    void SetStateInformation()
    {

        switch (currentState)
        {
            case CameraState.Static:
                StaticMode();
                break;
            case CameraState.SentryMode:
                SentryMode();
                break;
        }
    }

    void StaticMode()
    {

    }

    void SentryMode()
    {
        sentryTick += Time.deltaTime;
        //Gets a sin value between -1 and 1
        float rotationLerp = Mathf.Sin(sentryTick * rotationSpeed);
        //make the curve range between 0 and 1
        rotationLerp = (rotationLerp + 1) / 2;
        cam.transform.localEulerAngles = Vector3.Lerp(leftMostEulerRotation, rightMostEulerRotation, rotationLerp);
    }

    public void SetCameraState(CameraState _cameraState)
    {
        currentState = _cameraState;
    }
}
