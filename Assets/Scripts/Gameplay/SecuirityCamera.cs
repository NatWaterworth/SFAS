using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SecuirityCamera : ElectronicDevice
{
    public enum CameraState
    {
        Static, //Stays still
        SentryMode, //Moves side to side
        Detected // Focuses on a target.
          
    }

    [SerializeField]
    CameraState currentState;

    #region Sentry Mode Variables
    [Header("Sentry Mode")]
    [SerializeField] Vector3 leftMostEulerRotation;
    [SerializeField] Vector3 rightMostEulerRotation;
    [SerializeField][Range(0,1)] float rotationSpeed;
    [SerializeField] float sentryTick;

    #endregion

    #region Player Detection Variables
    [Header("Player Detection")]
    PlayerDetector detector;
    [SerializeField] float detectionRadius;
    [SerializeField] float viewAngle;
    bool foundPlayer;
    Transform detectedTarget;
    [SerializeField][Range(1,5)] float detectedFocusSpeed;
    #endregion

    [SerializeField] PostProcessVolume ppVolume;

    // Start is called before the first frame update
    protected override void Start()
    {
        resourcesDataPath = "Data/SecuirityCamera";

        base.Start();
        
        if (GetComponentInChildren<Camera>() != null)
        {
            deviceCamera = GetComponentInChildren<Camera>();
        }

        if (data == null)
        {
            Debug.LogError(this + " cannot find data at path: " + resourcesDataPath + ". Need data to change camera state.");
        }

        detector = new PlayerDetector();

        //Set rotation centre
        deviceCamera.transform.localEulerAngles = leftMostEulerRotation;
    }

    void SetCameraAlertPostProcess()
    {
        if (ppVolume != null)
        {
            Bloom bloomLayer = null;
            Vignette vignette = null;

            ppVolume.profile.TryGetSettings(out bloomLayer);
            ppVolume.profile.TryGetSettings(out vignette);

            bloomLayer.enabled.value = true;
            vignette.enabled.value = true;
            vignette.color.value = new Color(1, 0.3f, 0.3f); //red

            StartCoroutine(IncreasePostProcessIntensity(vignette));
        }
    }

    /// <summary>
    /// Over a short period of time increases intensity of vignette.
    /// </summary>
    /// <returns></returns>
    IEnumerator IncreasePostProcessIntensity(Vignette _vignette)
    {
        float _intensity = _vignette.intensity.value;
        float _increment = 0.02f;

        while (_intensity < 1)
        {
            _intensity += _increment;
            _vignette.intensity.value = _intensity;
            yield return new WaitForSeconds(0.02f);
        }
        _vignette.intensity.value = 1;

        yield return null;
    }
    // Update is called once per frame
    void Update()
    {
        SetStateInformation();
     
        //Look out for the player
        Transform target = detector.DetectPlayer(deviceCamera.transform, Vector3.zero, detectionRadius, viewAngle, this.name);

        //Found Player and focused on them (END STATE)
        if (target != null && !foundPlayer)
        {
            detectedTarget = target;
            foundPlayer = true;
            currentState = CameraState.Detected;
            SetCameraAlertPostProcess();
        }

    }

    public override void UpdateDeviceState(string _stateInfo)
    {
        base.UpdateDeviceState(_stateInfo);

        if (_stateInfo.Contains("static"))
            SetCameraState(CameraState.Static);
        else if (_stateInfo.Contains("sentry mode"))
            SetCameraState(CameraState.SentryMode);

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
            case CameraState.Detected:
                DetectedMode();
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
        deviceCamera.transform.localEulerAngles = Vector3.Lerp(leftMostEulerRotation, rightMostEulerRotation, rotationLerp);
    }

    void DetectedMode()
    {
        if (detectedTarget != null)
        {
            Debug.Log(this + ": looking at player");
            Vector3 targetDirection = (detectedTarget.position - deviceCamera.transform.position);
            float angle = Vector3.Angle(targetDirection, deviceCamera.transform.forward); 
            float step = detectedFocusSpeed * Time.deltaTime * Mathf.Clamp01(angle/(viewAngle/2));
            
            Vector3 nextForward = Vector3.RotateTowards(deviceCamera.transform.forward, targetDirection, step, 0.0f);
            deviceCamera.transform.forward = nextForward;
        }
        else
        {
            Debug.LogError(this + " does not have a target to detect!");
        }
    }

    public void SetCameraState(CameraState _cameraState)
    {
        currentState = _cameraState;
    }
}
