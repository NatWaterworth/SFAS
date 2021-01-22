using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    AsyncOperation loadScene;


    [Header("Loading Screen")]
    [SerializeField] [Tooltip("Added delay to loading time (so player can see bar load up if it's too quick)")] float loadingDelay = 1f;
    [SerializeField] Slider loadingBar;
    [SerializeField] TextMeshProUGUI loadingMessage;
    [SerializeField] TextMeshProUGUI loadingTitle;
    [SerializeField] Button continueButton;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] Color disabledButtonColour, enabledButtonColour;



    //Playthrough Timings
    float currentTotalPlayTime = 0;
    float bestTotalPlayTime;

    //used to check if the player wishes to continue.
    bool playerContinue;

    void Awake()
    {
        instance = this;

        SetStartingScene();

        if (continueButton == null)
            Debug.LogError(this + " has not loading screen button assigned to continue.");
    }

    public enum SceneIndex
    {
        Manager = 0,
        MainMenu = 1,
        Level1 = 2,
        Level2 = 3,
        EndGame = 4
    }

    #region Scene Management
    [Header("Scene Management")]
    [SerializeField] GameObject loadingScreen;

    [SerializeField] SceneIndex previousScene, currentScene;
    // [SerializeField] ProgressBar bar;
    float totalSceneProgress;
    #endregion

    /// <summary>
    /// Loads the game into the default starting scene.
    /// </summary>
    void SetStartingScene()
    {
        SceneManager.LoadSceneAsync((int)SceneIndex.MainMenu, LoadSceneMode.Additive);
        currentScene = SceneIndex.MainMenu;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayMusic("Menu Music");
        }
    }

    public void SetTimePaused(bool _paused)
    {
        if (_paused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void PlayerInvokeContinue(bool _continue)
    {
        playerContinue = _continue;
    }

    /// <summary>
    /// Determines whether player has confirmed to continue at a point which requires player response.
    /// </summary>
    /// <returns> players response to continue as True or False.</returns>
    bool PlayerContinue()
    {
        if (playerContinue)
        {
            //Reset continue to false for next required response.
            PlayerInvokeContinue(false);
            return true;
        }
        return false;
    }

    public void LoadScene(SceneIndex scene)
    {
        previousScene = currentScene;
        currentScene = scene;

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            SetLoadingTitle(currentScene.ToString());
            scenesLoading.Clear();
            scenesLoading.Add(SceneManager.UnloadSceneAsync((int)previousScene));

            loadScene = SceneManager.LoadSceneAsync((int)currentScene, LoadSceneMode.Additive);
            scenesLoading.Add(loadScene);
            loadScene.allowSceneActivation = false;

            StartCoroutine(GetSceneLoadingProgress());
        }
        else
            Debug.LogError(this+ " has no Loading Screen set!");
    }

    /// <summary>
    /// Load the next level (Based on GameManager SceneIndex).
    /// </summary>
    public void NextLevel()
    {
        SceneIndex theEnum = currentScene;
        int toInt = (int)theEnum + 1;
        SceneIndex nextScene = (SceneIndex)toInt;

        if (nextScene.Equals(SceneIndex.EndGame))
        {
            if(bestTotalPlayTime > currentTotalPlayTime || bestTotalPlayTime == 0)
                bestTotalPlayTime = currentTotalPlayTime;
            //Return to Main Menu for now.
            nextScene = SceneIndex.MainMenu;
        }

        LoadScene(nextScene);
    }

    public void ReloadLevel()
    {
        LoadScene(currentScene);
    }

    public void SetCursorVisible(bool _visible)
    {
        Cursor.visible = _visible;
    }

    void SetLoadingTitle(string levelName)
    {
        if (loadingTitle != null)
        {
            loadingTitle.text = "Hackerman - " + levelName + ".exe";
        }
        else
            Debug.LogError(this + " has no loading title GUI set for loading screen");
    }

    /// <summary>
    /// Update the loading screen load percentage and displayed message.
    /// </summary>
    /// <param name="_percentage">Ammount the level has loaded as a percentage (0-100%)</param>
    void UpdateLoadingScreen(float _percentage, string _message)
    {
        if (loadingBar != null)
        {
            _percentage = Mathf.Clamp(_percentage, 0, 100);
            loadingBar.value = _percentage;
        }
        else
            Debug.LogError(this+" has no loading bar GUI set for loading screen");
        if (loadingMessage != null)
        {
            loadingMessage.text = _message;
        }
        else
            Debug.LogError(this + " has no loading message GUI set for loading screen");

        if (continueButton != null)
        {
            if (_percentage != 100)
            {
                continueButton.image.color = disabledButtonColour;
                continueButton.enabled = false;
            }
            else
            {
                continueButton.image.color = enabledButtonColour;
                continueButton.enabled = true;
            }
        }
        else
            Debug.LogError(this + " has no button GUI set for loading screen");

        if (buttonText != null)
        {
            if (_percentage != 100)
                buttonText.color = disabledButtonColour;
            else
                buttonText.color = enabledButtonColour;
        }
        else
            Debug.LogError(this + " has no button text GUI set for loading screen");
    }

    public IEnumerator GetSceneLoadingProgress()
    {
        SetCursorVisible(true);
        //Reset Loading bar.
        UpdateLoadingScreen(0, "Loading...");
        yield return new WaitForSeconds(loadingDelay);

        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone && !IsNextSceneReady(scenesLoading[i]))
            {
                totalSceneProgress = 0;

                foreach(AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }
                Debug.Log(scenesLoading[i]+ " progress" + totalSceneProgress);

                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100f;

                UpdateLoadingScreen(totalSceneProgress, "Loading...");

                yield return null;
            }        
        }
      
        totalSceneProgress = 100; //complete.
        UpdateLoadingScreen(totalSceneProgress, "Complete.");


        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySoundEffect("Select");
        }

        //Press button to continue (otherwise loading screen moves too quick).
        if (continueButton != null)
        {
            while (!PlayerContinue())
            {
                yield return null;
            }
        }
        else //Press any key to continue (otherwise loading screen moves too quick).
        {
            while (!Input.anyKeyDown)
            {
                yield return null;
            }
        }

        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopMusic("Menu Music");
        }

        //SetCursorVisible(false);

        //Activate Scene now player is ready.
        if (loadScene != null)
            loadScene.allowSceneActivation = true;
        else
            Debug.LogError("LoadScene is NULL! Cannot allow scene activation.");

        //Turn off loading screen
        loadingScreen.SetActive(false);
    }


    public float GetTotalLevelTime()
    {
        return currentTotalPlayTime;
    }

    public float GetBestTotalTime()
    {
        return bestTotalPlayTime;
    }

    public void SetTotalLevelTime(float _totalLevelTime)
    {
        _totalLevelTime = Mathf.Round(_totalLevelTime * 100f) / 100f;
        currentTotalPlayTime = _totalLevelTime;
    }

    /// <summary>
    /// Used to determine if scene load progress has reached 0.9.
    /// </summary>
    /// <param name="operation">Scene to load/unload.</param>
    /// <returns> True if scene is ready for activation.</returns>
    bool IsNextSceneReady(AsyncOperation operation)
    {
        /* Unity Docs
         *
         * When allowSceneActivation is set to false then progress is stopped at 0.9.
         * The isDone is then maintained at false. When allowSceneActivation is set
         * to true isDone can complete. While isDone is false, the AsyncOperation 
         * queue is stalled. For example, if a LoadSceneAsync.allowSceneActivation
         * is set to false, and another AsyncOperation (e.g. SceneManager.UnloadSceneAsync )
         * is initialized, the last operation will not be called before the first
         * allowSceneActivation is set to true. */

        if (operation.Equals(loadScene))
        {
            Debug.Log("Loading Progress:" + loadScene.progress);
            if (loadScene.progress >= 0.9f)
            {
                Debug.Log("Done Loading:" + loadScene);
                return true;
            }
        }
        return false;
    }


    public void ExitGame()
    {
        Application.Quit();
    }

}
