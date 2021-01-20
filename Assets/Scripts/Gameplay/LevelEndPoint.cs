using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndPoint : MonoBehaviour
{
    bool reachedEndPoint;

    void Start()
    {
        reachedEndPoint = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //player reached end
        if (other.GetComponent<Player>() != null)
        {
            reachedEndPoint = true;
        }
    }

    public bool IsLevelComplete()
    {
        return reachedEndPoint;
    }

}
