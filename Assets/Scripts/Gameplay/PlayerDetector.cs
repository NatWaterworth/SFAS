using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector
{
    Transform playerTransform;
    Vector3 playerCentreOffset = new Vector3(0,2f,0);
    float playerWidth =.4f;
    float maxAngle = 130f;
    /// <summary>
    /// Detects if the player is within line of sight of detector.
    /// Returns True if raycast was successful.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="detector"></param>
    public void DetectPlayer(Transform detector,Vector3 offset, float viewRange)
    {
        if (playerTransform == null)
        {       
            FindAndSetPlayerTransform();
            if (playerTransform == null)
                return;
        }

        Vector3 detectorPosition = detector.position + offset;
        Vector3 playerPosition = playerTransform.position + playerCentreOffset;

        Vector3 direction = (playerPosition - detectorPosition);
        Vector3 left = Vector3.Cross(direction, Vector3.up).normalized;

        //Only be able to detect in a direction within viewing range of forward direction
        float angle = Vector3.Angle(detector.forward, direction);

        if (angle > (maxAngle/2))
            return;

        //Do a fan of raycasts to help detect non cnetral bodyparts
        RaycastHit hit;

        for (int i = -2; i < 3; i++)
        {

            playerPosition = playerTransform.position + playerCentreOffset + (left * playerWidth * i);
            
            if (Physics.Raycast(detectorPosition, (playerPosition - detectorPosition).normalized, out hit, viewRange))
            {
                if (hit.collider.transform.root.GetComponent<Player>() != null)
                {
                    Debug.DrawRay(detectorPosition, (playerPosition - detectorPosition).normalized * hit.distance, Color.green);
                    PlayerDetected();
                }
                else
                    Debug.DrawRay(detectorPosition, (playerPosition - detectorPosition).normalized * hit.distance, Color.red);
                //report
            }
            else
            {
                Debug.DrawRay(detector.position, (playerPosition - detector.position).normalized * viewRange, Color.white);
            }
        }
    }

    void PlayerDetected()
    {
        if (GameObject.FindObjectOfType<LevelManager>() != null)
        {
            GameObject.FindObjectOfType<LevelManager>().PlayerDetected();
        }
    }

    /// <summary>
    /// Finds first player in scene and assigns it as the player to detect.
    /// </summary>
    /// <returns></returns>
    public bool FindAndSetPlayerTransform()
    {
        if (GameObject.FindObjectOfType<Player>() != null)
        {
            playerTransform = GameObject.FindObjectOfType<Player>().transform;
            return true;
        }
        else
        {
            Debug.LogError(this + " couldn't find the player!");
            return false;
        }
    }
}
