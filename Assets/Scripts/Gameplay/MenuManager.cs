using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadGame();
        }
        else
            Debug.LogError(this + " cannot find instance of Game Manager");
    }
}
