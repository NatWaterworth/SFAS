using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("More than 1 instance of level manager exists!");
    }

    [System.Serializable]
    struct MenuSet
    {
        public string name;
        public GameObject menuParent;
    }

    [Header("Menu Sets")]
    [SerializeField] MenuSet[] menuSets;

    public enum GameState
    {
        Intro,
        Playing,
        Paused,
        HowToPlayScreen,
        Caught
    }

    [Header("Level Information")]
    [SerializeField] GameState currentState;
    [SerializeField] bool acceptingInput;
    [SerializeField] StoryData levelData;

    float totalLevelTime;
    [SerializeField] TextMeshProUGUI timeText;

    #region Camera Work
    [Header("Cameras")]
    [SerializeField] CinematicCamera cinematicCamera;
    [SerializeField] PlayerCamera playerCamera;
    [SerializeField] Transitioner transitioner;

    [SerializeField]
    [Tooltip("The value set in Inspector will be the default if the Cinematic Camera doesn't provide a value.")]
    float cinematicTime;
    float hackerTypingTime = 3f;
    #endregion

    #region Level Fundimentals
    [Header("Level Fundimentals")]
    [SerializeField] MiniMapSelector miniMap;
    [SerializeField] Hacker hacker;
    [SerializeField] Game console;
    [SerializeField] GameObject endStateScreen;
    [SerializeField] TextMeshProUGUI[] stateMessages;
    [SerializeField] LevelEndPoint EndLevelArea;
    [SerializeField] Player player;
    bool detected;
    bool complete;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        CheckCameras();
        SwitchCurrentState(GameState.Intro);
        SetUpLevel();

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayMusic("Sneak Music");
        }

    }

    // Update is called once per frame
    void Update()
    {
        RunCurrentState();       
    }

    void SetUpLevel()
    {
        SetStartingLevelTime();

        endStateScreen.SetActive(false);
        detected = false;

        //Check fundimental gameobjects have been set.

        if (console == null)
            Debug.LogError(this + " has no console set!");
        if (player == null)
            Debug.LogError(this + " has no Player set!");
        if (miniMap == null)
            Debug.LogError(this + " has no miniMap set!");

    }

    /// <summary>
    /// Gets the total level time so far and sets it as the starting time.
    /// </summary>
    void SetStartingLevelTime()
    {
        totalLevelTime = 0;

        if (GameManager.instance != null)
        {
            totalLevelTime = GameManager.instance.GetTotalLevelTime();
            if(timeText!=null)
                timeText.text = "Time: " + totalLevelTime.ToString();
        }
    }

    void IncrementLevelTime()
    {
        if (timeText != null)
        {
            timeText.text = "Time: " + (((float)Mathf.Round(totalLevelTime*100))/100).ToString();
            totalLevelTime += Time.deltaTime;
        }
    }
     
    /// <summary>
    /// Manages the inputs for navigating and activating Menus.
    /// </summary>
    void ManageMenuInput()
    {
        if (Input.GetButtonDown("Escape"))
        {
            //Manage what pressing Escape does in each state.
            switch (currentState)
            {
                case GameState.Playing:
                    SwitchCurrentState(GameState.Paused);
                    break;

                case GameState.Paused:
                    SwitchCurrentState(GameState.Playing);
                    break;

                case GameState.HowToPlayScreen:
                    SwitchCurrentState(GameState.Paused);
                    break;
            }
        }
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
                SetLevelInteractions(false);
                break;

            case GameState.Playing:
                DisplayHUD();
                SetLevelInteractions(true);
                break;

            case GameState.Paused:
                DisplayPauseMenu();
                SetLevelInteractions(false);
                break;

            case GameState.HowToPlayScreen:
                DisplayHowToPlayScreen();
                SetLevelInteractions(false);
                break;
        }
    }

    public void GoToPlayState()
    {
        SwitchCurrentState(GameState.Playing);
    }

    /// <summary>
    /// Displays Pause Menu (Turns off other active menu's on use).
    /// </summary>
    public void DisplayPauseMenu()
    {
        SetActiveUI("Pause Menu");
    }

    /// <summary>
    /// Displays How to Play Screen (Turns off other active menu's on use).
    /// </summary>
    public void DisplayHowToPlayScreen()
    {
        SetActiveUI("How To Play Screen");
    }

    /// <summary>
    /// Displays HUD (Turns off other active menu's on use).
    /// </summary>
    public void DisplayHUD()
    {
        SetActiveUI("");
    }


    void SetActiveUI(string uiSetName)
    {
        int matches = 0;
        foreach (MenuSet set in menuSets)
        {
            if (uiSetName.Equals(set.name))
            {
                set.menuParent.SetActive(true);
                matches++;
            }
            else
                set.menuParent.SetActive(false);
        }

        if (matches != 1 && uiSetName == "")
        {
            Debug.LogWarning(this + " found an unexpected number of UI Sets to activate: " + matches);
        }
    }

    /// <summary>
    /// Restarts level as the player was detected (player failed level).
    /// </summary>
    public void PlayerDetected()
    {
        if (!detected)
        {
            detected = true;
            StartCoroutine(LevelFailed());

            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySoundEffect("Alert");
            }
        }
        
    }

    void CheckLevelComplete()
    {
        if (EndLevelArea.IsLevelComplete() && !complete)
        {
            complete = true;

            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySoundEffect("Complete");
            }

            StartCoroutine(LevelComplete());
        }
    }

    IEnumerator LevelFailed()
    {
        float transitionTime = 4f;
        float opaqueTime = .8f;

        foreach(TextMeshProUGUI gui in stateMessages)
        {
            gui.text = "Detected!";
            gui.color = new Color(1, 0, 0, gui.color.a);
        }

        endStateScreen.SetActive(true);
        transitioner.MaskTransition(transitionTime, 0.05f, opaqueTime, false);
        yield return new WaitForSeconds(transitionTime/2);

        if (GameManager.instance != null)
        {
            GameManager.instance.ReloadLevel();
        }
        else
        {
            Debug.LogError("Couldn't find Game Manager to set next scene.");
        }
    }

    IEnumerator LevelComplete()
    {
        float transitionTime = 2f;
        float opaqueTime = 1f;

        foreach (TextMeshProUGUI gui in stateMessages)
        {
            gui.text = "Level Complete!";
            gui.color = new Color(0, 1, 0, gui.color.a);
        }

        endStateScreen.SetActive(true);
        transitioner.MaskTransition(transitionTime, 0.05f, opaqueTime, false);
        yield return new WaitForSeconds(transitionTime / 2);

        if (GameManager.instance != null)
        {
            GameManager.instance.SetTotalLevelTime(totalLevelTime);
            GameManager.instance.NextLevel();
        }
        else
        {
            Debug.LogError("Couldn't find Game Manager to set next scene.");
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
            Debug.LogWarning(this + ": Hacker hasn't been set. Animating Hacker could not be executed.");
        }
    }

    /// <summary>
    /// Sets level interactions to True or False.
    /// </summary>
    /// <param name="_acceptingInput"></param>
    void SetLevelInteractions(bool _acceptingInput)
    {
        acceptingInput = _acceptingInput;

        //Ensure player can't interact with map before intro is complete.
        if (miniMap != null)
            miniMap.SetTakingInput(acceptingInput);

        if (console != null)
            console.SetAcceptingInput(acceptingInput);

        if (player != null)
            player.SetAcceptingInput(acceptingInput);
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
            case GameState.Playing:
                IncrementLevelTime();
                CheckLevelComplete();
                ManageMenuInput();
                break;
            case GameState.Paused:
                ManageMenuInput();
                break;
            case GameState.HowToPlayScreen:
                ManageMenuInput();
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
        StartCoroutine(IntroScene());
    }

    IEnumerator IntroScene()
    {
        yield return new WaitForSeconds(1);

        if (cinematicCamera.GetCinematicTime() > 0)
            cinematicTime = cinematicCamera.GetCinematicTime();

        if (transitioner != null)
        {
            float transitionTime = 1f;
          
            SetCameraToPlayer(false);           
            SetHackerIntroAnimation();

            //Sync up with transition so when fully masked it changes camera.
            yield return new WaitForSeconds(cinematicTime - (transitionTime / 2));
            transitioner.MaskTransition(transitionTime, 0.05f, 0.2f, false);
            yield return new WaitForSeconds(transitionTime / 2);

            SetCameraToPlayer(true);

            // if(hackerTypingTime > 0)
            //     yield return new WaitForSeconds(hackerTypingTime);

            playerCamera.SetCameraToFocused(true);

            //Switch to Main menu after Game Intro
            SwitchCurrentState(GameState.Playing);

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
