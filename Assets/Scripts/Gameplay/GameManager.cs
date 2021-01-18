using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused,
        MainMenu
    }

    [SerializeField] GameState currentState;

    #region Camera Work
    [Header("Cameras")]
    [SerializeField] CinematicCamera cinematicCamera;
    [SerializeField] PlayerCamera playerCamera;
    [SerializeField] Transitioner transitioner;

    [SerializeField] [Tooltip("The value set in Inspector will be the default if the Cinematic Camera doesn't provide a value.")]
    float cinematicTime;
    #endregion



    // Start is called before the first frame update
    void Start()
    {
        CheckCameras();
        CinematicIntro();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Error Checking Camera's have been set.
    /// </summary>
    void CheckCameras()
    {
        if (cinematicCamera == null)
        {
            Debug.LogError(this + " doesn't have a Cinematic Camera set up!");
        }

        if (playerCamera == null)
        {
            Debug.LogError(this + " doesn't have a Player Camera set up!");
        }

    }

    /// <summary>
    /// Plays Cinematic Intro to the game.
    /// </summary>
    void CinematicIntro()
    {
        if (cinematicCamera.GetCinematicTime() > 0)
            cinematicTime = cinematicCamera.GetCinematicTime();

        StartCoroutine(IntroScene());
    }

    IEnumerator IntroScene()
    {
        if (transitioner != null)
        {
            float transitionTime = 1f;
           
            SetCameraToPlayer(false);

            //Sync up with transition so when fully masked it changes camera.
            yield return new WaitForSeconds(cinematicTime - (transitionTime / 2));
            transitioner.MaskTransition(transitionTime, 0.05f, 0.2f, false);
            yield return new WaitForSeconds(transitionTime / 2);

            SetCameraToPlayer(true);
        }
        else
        {
            Debug.LogError("Gamemanager  (" + this + ") transitioner is null!");
        }
        yield return null;
    }  

    /// <summary>
    /// Sets active camera in game to either Cinematic or Player camera.
    /// </summary>
    /// <param name="_active">If set to true, player camera is active.</param>
    void SetCameraToPlayer(bool _active)
    {
        Debug.Log("Setting player camera: " + _active);
        cinematicCamera.SetCameraActive(!_active);
        playerCamera.SetCameraActive(_active);
    }
}
