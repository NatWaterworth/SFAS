using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A visual component for immitating a Hacker with player inputs controlling animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class Hacker : MonoBehaviour
{
    [SerializeField] bool animate;
    [SerializeField] Transform bodyPart;
    [SerializeField] Transform rightHandHoldPoint, mouseStationaryPoint;

    [SerializeField] [Tooltip("World position.")]Transform ObjectCentralStartingPoint;
    Vector3 mouseCentrePoint;
    Vector3 bodyPartStartingPoint;
    [SerializeField] [Tooltip("Local position of mouse.")]Vector2 minMousePosition, maxMousePosition;
    [SerializeField] MiniMapSelector mouseController;

    [SerializeField] Transform introTransform;
    Transform playingTransform;

    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };
 
    // Start is called before the first frame update
    void Start()
    {
        SetUpHackerVisuals();
    }

    /// <summary>
    /// Apply animation updates here.
    /// </summary>
    void LateUpdate()
    {
        if (!animate)
            return;

        UpdateArmPosition(mouseController.GetScreenPosition());

        //Animate for keyboard input
        AnimateKeyPress(CheckNumericalKeyboardInput());
        //Animate for mouse input
        AnimateMouseClick(Input.GetMouseButtonDown(0)); // 0 - left click



    }

    /// <summary>
    /// Checks if an input is a key from 0-9 on the keyboard
    /// </summary>
    bool CheckNumericalKeyboardInput()
    {
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                return true;
            }
        }
        return false;
    }


    void SetUpHackerVisuals()
    {
        mouseCentrePoint = ObjectCentralStartingPoint.position;
        bodyPartStartingPoint = bodyPart.position;       
        playingTransform = transform;
        if (introTransform != null)
            transform.position = introTransform.position;
        else
            Debug.LogWarning(this + " hasn't set an intro animation Transform.");
    }

    /// <summary>
    /// Sets animation to be responsive to player inputs. If false, animations must be called externally to play.
    /// </summary>
    /// <param name="_isAnimating"></param>
    public void SetAnimationToUpdate(bool _isAnimating)
    {
        animate = _isAnimating;
    }


    /// <summary>
    /// Make the Hacker appear to be typing.
    /// </summary>
    /// <param name="_typing">Whilst True hacker will animate typing.</param>
    void AnimateKeyPress(bool _typing)
    {
        if (SoundManager.instance != null && _typing)
        {
            SoundManager.instance.PlaySoundEffect("Type");
        }

        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetBool("Type", _typing);
        else
            Debug.LogWarning(this + ": Animator not set.");
    }

    /// <summary>
    /// Make the Hacker appear to be clicking mouse
    /// </summary>
    /// <param name="_click">Whilst True hacker will animate clicking mouse.</param>
    void AnimateMouseClick(bool _click)
    {
        if (SoundManager.instance != null && _click)
        {
            SoundManager.instance.PlaySoundEffect("Click");
        }

        //return;

        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetBool("Click", _click);
        else
            Debug.LogWarning(this + ": Animator not set.");
    }

    /// <summary>
    /// Make the Hacker appear to be engrossed in his computer action.
    /// </summary>
    /// <param name="_intro">Whilst True hacker will animate laptop usage.</param>
    public void AnimateHackerIntro(bool _intro)
    {
        if (SoundManager.instance != null)
        {
            if(_intro)
                SoundManager.instance.PlaySoundEffect("Typing");
            else
                SoundManager.instance.StopSoundEffect("Typing");
        }

        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetBool("Intro", _intro);
        else
            Debug.LogWarning(this + ": Animator not set.");

        if (introTransform != null)
        {
            if (_intro)
            {
                transform.position = playingTransform.position;
            }
            else
            {
                transform.position = introTransform.position;
            }
        }

        if (mouseStationaryPoint != null)
        {
            ObjectCentralStartingPoint.parent = mouseStationaryPoint;
            ObjectCentralStartingPoint.localPosition = Vector3.zero;
            ObjectCentralStartingPoint.localEulerAngles = Vector3.zero;
        }
    }


    /// <summary>
    /// Update arm position based on where Vector2 percentagePos
    /// </summary>
    void UpdateArmPosition(Vector2 percentagePos)
    {
        if (mouseController == null)
        {
            Debug.LogError(this + " doesn't have a MiniMapSelector Set!");
            return;
        }

        if (mouseController.IsTakingInput())
        {
            float x = Mathf.Lerp(minMousePosition.x, maxMousePosition.x, percentagePos.x);
            float z = Mathf.Lerp(minMousePosition.y, maxMousePosition.y, percentagePos.y);

            bodyPart.position = bodyPartStartingPoint + new Vector3(x, 0, z);

            if (rightHandHoldPoint != null)
            {
                ObjectCentralStartingPoint.parent = rightHandHoldPoint;
                ObjectCentralStartingPoint.localPosition = Vector3.zero;
                ObjectCentralStartingPoint.localEulerAngles = Vector3.zero;
            }
        }
    }
}
