using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Electronic device that can be triggered to alert a guard.
/// </summary>
public class Alarm : ElectronicDevice
{
    #region Alarm Mechanics
    [Header("Alarm Mechanics")]
    [SerializeField] float guardAlertDelay = 1f;
    [SerializeField] float alarmTurnOffDelay = .5f;
    [SerializeField] BoxCollider detectionCollider;
    [SerializeField] Transform turnOffPoint;
    bool triggered;
    #endregion

    #region Alarm Visuals
    [Header("Alarm Visuals")]
    [SerializeField] Light alarmLight;
    [SerializeField] MeshRenderer alarmIndicator;
    [SerializeField] Material alertMaterial, standbyMaterial;
    #endregion

    #region Alarm UI 
    [Header("UI Variables")]
    [SerializeField] Image alarmSymbol;
    [SerializeField] Color alertColour, standbyColour;
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        resourcesDataPath = "Data/Alarm";
        base.Start();
        SetAlarmUp();
    }

    void Update()
    {
        if(triggered)
            CheckForGuard();
    }

    void SetAlarmUp()
    {
        //Ensure Alarm is set to silent mode
        triggered = true;
        TriggeredAlarm(false);
    }

    public override void UpdateDeviceState(string _stateInfo)
    {
        base.UpdateDeviceState(_stateInfo);

        Debug.Log("State Info: " + _stateInfo);
        if (_stateInfo.Contains("Ringing"))
            TriggeredAlarm(true);
        else if (_stateInfo.Contains("Silenced"))
            TriggeredAlarm(false);
    }

    IEnumerator AlertClosestGuard()
    {
        Vector3 _destination;

        if (turnOffPoint == null)
        {
            Debug.LogWarning(this + " has no set turn off point!");
            _destination = transform.position;
        }
        else
            _destination = turnOffPoint.position;

        guardAlertDelay = Mathf.Max(guardAlertDelay, 0.1f); //Ensure delay isn't less than 0.1s
        yield return new WaitForSeconds(guardAlertDelay);

        Guard[] _guards = FindObjectsOfType<Guard>();
        Debug.Log("guards found: "+_guards.Length);
        if (_guards.Length >= 1)
        {

            Guard closestGaurd = _guards[0];
            float closestDistance = Mathf.Infinity;

            foreach (Guard guard in _guards)
            {
                float distance = guard.GetTravelDistanceToPoint(_destination);
                Debug.Log("guards distance: " + distance + " - "+guard.name);
                if (distance <= closestDistance)
                {
                    closestGaurd = guard;
                    closestDistance = distance;
                }
            }

            if (closestGaurd != null)
            {
                closestGaurd.Investigate(_destination);
            }
        }
        else
            Debug.LogWarning(this + " found no Gaurds to alert!");
    }

    void TriggeredAlarm(bool _triggered)
    {
        //if state has not changed return
        if (triggered == _triggered)
            return;

        triggered = _triggered;

        if(triggered)
            StartCoroutine(AlertClosestGuard());

        if (SoundManager.instance != null)
        {
            if(triggered)
                SoundManager.instance.PlaySoundEffect("Alarm");
            else
                SoundManager.instance.StopSoundEffect("Alarm");

            SoundManager.instance.PlaySoundEffect("Select");
        }

        if (alarmSymbol != null)
        {
            if (triggered)
            {
                alarmSymbol.color = alertColour;
            }
            else
            {
                alarmSymbol.color = standbyColour;
            }
        }
        else
        {
            Debug.LogWarning(this + " doesn't have an locked Symbol set!");
        }

        if (alarmLight != null)
        {
            if (triggered)
            {
                alarmLight.color = alertColour;
            }
            else
            {
                alarmLight.color = standbyColour;
            }
        }
        else
        {
            Debug.LogWarning(this + " doesn't have an indicator light set!");
        }

        if (alarmIndicator != null && alertMaterial != null && standbyMaterial != null)
        {
            if (triggered)
            {
                alarmIndicator.material = alertMaterial;
            }
            else
            {
                alarmIndicator.material = standbyMaterial;
            }
        }
        else
        {
            Debug.LogWarning(this + " doesn't have an indicator variable set!");
        }
    }

    /// <summary>
    /// Once guard get's into area turn off the alarm.
    /// </summary>
    void CheckForGuard()
    {
        if (detectionCollider == null)
        {
            Debug.LogError(this + " has no detection collider set.");
            return;
        }

        Collider[] overlappedColliders = Physics.OverlapBox(detectionCollider.transform.position + detectionCollider.center, detectionCollider.size);

        foreach (Collider col in overlappedColliders)
        {
            if (col.GetComponent<Guard>() != null)
            {
                StartCoroutine(TurnOffAlarm());
                return;
            }
        }
    }

    /// <summary>
    /// Turn off the alarm after guardAlertDelay.
    /// </summary>
    /// <returns></returns>
    IEnumerator TurnOffAlarm()
    {
        alarmTurnOffDelay = Mathf.Max(alarmTurnOffDelay, 0.1f); //Ensure delay isn't less than 0.1s
        yield return new WaitForSeconds(alarmTurnOffDelay);
        TriggeredAlarm(false);
    }
}
