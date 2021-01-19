using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    void Awake()
    {
        instance = this;

        SceneManager.LoadSceneAsync((int)SceneIndex.MainMenu, LoadSceneMode.Additive);
    }

    public enum SceneIndex
    {
        Manager = 0,
        MainMenu = 1,
        Level1 = 2,
    }

    #region Scene Management
    [Header("Scene Management")]
    [SerializeField] GameObject loadingScreen;
    // [SerializeField] ProgressBar bar;
    float totalSceneProgress;
    #endregion

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.Q))
        {
            LoadGame();
        }
    }

    public void LoadGame()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            scenesLoading.Clear();
            scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneIndex.MainMenu));
            scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneIndex.Level1,LoadSceneMode.Additive));
            StartCoroutine(GetSceneLoadingProgress());
        }
        else
            Debug.LogError(this+ " has no Loading Screen set!");
    }

    public IEnumerator GetSceneLoadingProgress()
    {
        for(int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                foreach(AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }

                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100f;

                yield return null;
            }
        }

        //once loading is finished
        loadingScreen.SetActive(false);
    }
}
