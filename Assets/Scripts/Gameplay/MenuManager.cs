using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI bestScoreText;

    [System.Serializable]
    struct MenuSet
    {
        public string name;
        public GameObject menuParent;
    }

    [Header("Menu Sets")]
    [SerializeField] MenuSet[] menuSets;

    // Start is called before the first frame update
    void Start()
    {
        SetBestScore();
    }

    /// <summary>
    /// Sets UI to represent the players best playthrough time.
    /// </summary>
    void SetBestScore()
    {
        if (GameManager.instance == null)
        {
            Debug.LogWarning(this + " found no instance of Game Manager.");
            return;
        }

        if (bestScoreText == null)
        {
            Debug.LogWarning(this + " has no TextMeshProGUI set to display best score.");
            return;
        }
        if(GameManager.instance.GetBestTotalTime()!= 0)
            bestScoreText.text = "Fastest Escape Time: " + GameManager.instance.GetBestTotalTime().ToString();
        else
            bestScoreText.text = "Fastest Escape Time: None";
    }

    public void PlayGame()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySoundEffect("Click");
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.LoadScene(GameManager.SceneIndex.Level1);
        }
        else
            Debug.LogError(this + " cannot find instance of Game Manager");
    }

    public void HoverSound()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySoundEffect("Hover");
        }
    }

    public void ClickSound()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySoundEffect("Click");
        }
    }

    /// <summary>
    /// Displays How to Play Screen (Turns off other active menu's on use).
    /// </summary>
    public void DisplayHowToPlayScreen()
    {
        SetActiveUI("How To Play Screen");
    }

    /// <summary>
    /// Displays How to Play Screen (Turns off other active menu's on use).
    /// </summary>
    public void DisplayMainMenu()
    {
        SetActiveUI("Main Menu");
    }

    public void ExitGame()
    {
        if (GameManager.instance != null)
            GameManager.instance.ExitGame();
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

}
