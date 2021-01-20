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
    /// </summary>
    /// <param name="_detector">Detection point of origin.</param>
    /// <param name="_offset">Offset position of detection from detector.</param>
    /// <param name="_viewRange">range in which the player can be detected.</param>
    /// <param name="_detectionAngle">Angle of detection. Outside this cannot be detected.</param>
    /// <param name="_detectorName">Name of detector for debugging.</param>
    /// <returns>Player Transform if raycast was successful.</returns>
    public Transform DetectPlayer(Transform _detector,Vector3 _offset, float _viewRange,float _detectionAngle, string _detectorName)
    {
        maxAngle = _detectionAngle;

        if (playerTransform == null)
        {       
            FindAndSetPlayerTransform();
            if (playerTransform == null)
            {
                Debug.LogError(_detectorName + ": "+ this + " - could not find a player to detect!");
                return null;
            }
        }

        Vector3 detectorPosition = _detector.position + _offset;
        Vector3 playerPosition = playerTransform.position + playerCentreOffset;

        Vector3 direction = (playerPosition - detectorPosition);
        Vector3 left = Vector3.Cross(direction, Vector3.up).normalized;

        //Only be able to detect in a direction within viewing range of forward direction
        float angle = Vector3.Angle(_detector.forward, direction);

        if (angle > (maxAngle/2))
            return null;

        //Do a fan of raycasts to help detect non central bodyparts
        RaycastHit hit;

        for (int i = -2; i < 3; i++)
        {

            playerPosition = playerTransform.position + playerCentreOffset + (left * playerWidth * i);
            //new direction using player width 
            direction = (playerPosition - detectorPosition);
            //ensure angle is within range
            angle = Vector3.Angle(_detector.forward, direction);

            if (angle <= (maxAngle / 2))
            {

                if (Physics.Raycast(detectorPosition, direction.normalized, out hit, _viewRange))
                {
                    if (hit.collider.transform.root.GetComponent<Player>() != null)
                    {
                        Debug.DrawRay(detectorPosition, (playerPosition - detectorPosition).normalized * hit.distance, Color.green);
                        PlayerDetected();
                        //Debug.Log(_detectorName + " detected player!");
                        return playerTransform;
                    }
                    else
                        Debug.DrawRay(detectorPosition, (playerPosition - detectorPosition).normalized * hit.distance, Color.red);
                    //report
                }
                else
                {
                    Debug.DrawRay(_detector.position, (playerPosition - _detector.position).normalized * _viewRange, Color.white);
                }
            }
        }
        return null;
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
