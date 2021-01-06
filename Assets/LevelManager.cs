using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    bool playerDetected;
    ElectronicDevice[] devices;


    // Start is called before the first frame update
    void Start()
    {
        devices = FindObjectsOfType<ElectronicDevice>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetActiveDevice(ElectronicDevice device)
    {

    }

    public void PlayerDetected(bool _detected)
    {
        playerDetected = _detected;
    }
}
