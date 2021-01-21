using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : CameraController
{
    [SerializeField] Transform playerHead;
    [SerializeField][Tooltip("The position which the camera will look at.")] Transform cameraFocalPoint;
    [SerializeField] bool overideHeadAnimation;
    [SerializeField] Vector3 desiredHeadPos;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        FocusedCamera();
    }
    /// <summary>
    /// Overides Transform camera is attached to in order to focus on a set point.
    /// </summary>
    public void SetCameraToFocused(bool _focused)
    {
        overideHeadAnimation = _focused;
    }

    /// <summary>
    /// Camera will focus on a focal point.
    /// </summary>
    void FocusedCamera()
    {
        if (!overideHeadAnimation)
            return;

        if (playerHead != null)
        {
            if (cameraFocalPoint != null)
                playerHead.LookAt(cameraFocalPoint.position);
            else
                Debug.LogWarning(this + " cannot focus on point as it hasn't been set.");

            //fixed head position
            playerHead.transform.position = new Vector3(desiredHeadPos.x, desiredHeadPos.y, desiredHeadPos.z);
        }

    }
}
