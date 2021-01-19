using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Intro,
        Playing,
        Paused,
        MainMenu,
        LevelSelection
    }

    [SerializeField] GameState currentState;
    [SerializeField] bool acceptingInput;

    #region Camera Work
    [Header("Cameras")]
    [SerializeField] CinematicCamera cinematicCamera;
    [SerializeField] PlayerCamera playerCamera;
    [SerializeField] Transitioner transitioner;

    [SerializeField] [Tooltip("The value set in Inspector will be the default if the Cinematic Camera doesn't provide a value.")]
    float cinematicTime;
    float hackerTypingTime = 3f;
    #endregion

    [SerializeField] MiniMapSelector miniMap;
    [SerializeField] Hacker hacker;

    // Start is called before the first frame update
    void Start()
    {       
        CheckCameras();
        SwitchCurrentState(GameState.Intro);       
    }

    // Update is called once per frame
    void Update()
    {
        RunCurrentState();
    }
    /// <summary>
    /// Called once when switching states in a game.
    /// </summary>
    /// <param name="_state">State to switch to.</param>
    void SwitchCurrentState(GameState _state)
    {
        currentState = _state;

        switch (currentState)
        {
            case GameState.Intro:
                CinematicIntro();
                break;
            case GameState.LevelSelection:
                break;
            case GameState.MainMenu:
                break;
            case GameState.Playing:
                break;
            case GameState.Paused:
                break;

        }
    }


    void SetHackerIntroAnimation()
    {
        if (hacker != null)
        {
            if (currentState.Equals(GameState.Intro))
            {
                hacker.AnimateHackerIntro(true);
                hacker.SetAnimationToUpdate(false);
            }
            else
            {
                hacker.AnimateHackerIntro(false);
                hacker.SetAnimationToUpdate(true);
            }
        }
        else
        {
            Debug.LogWarning(this+": Hacker hasn't been set. Animating Hacker could not be executed.");
        }
    }

    /// <summary>
    /// Called every frame for running current state updates.
    /// </summary>
    void RunCurrentState()
    {
        switch (currentState)
        {
            case GameState.Intro:
                break;
            case GameState.LevelSelection:
                break;
            case GameState.MainMenu:
                break;
            case GameState.Playing:
                break;
            case GameState.Paused:
                break;
        }
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
            acceptingInput = false;
           
            SetCameraToPlayer(false);

            SetHackerIntroAnimation();

            //Ensure player can't interact with map before intro is complete.
            if (miniMap != null)
                miniMap.SetTakingInput(acceptingInput);

            //Sync up with transition so when fully masked it changes camera.
            yield return new WaitForSeconds(cinematicTime - (transitionTime / 2));
            transitioner.MaskTransition(transitionTime, 0.05f, 0.2f, false);
            yield return new WaitForSeconds(transitionTime / 2);

            SetCameraToPlayer(true);

           // if(hackerTypingTime > 0)
           //     yield return new WaitForSeconds(hackerTypingTime);

            playerCamera.SetCameraToFocused(true);

            acceptingInput = true;

            //Ensure player can now interact
            if (miniMap != null)
                miniMap.SetTakingInput(acceptingInput);

            //Switch to Main menu after Game Intro
            SwitchCurrentState(GameState.MainMenu);

            SetHackerIntroAnimation();
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
