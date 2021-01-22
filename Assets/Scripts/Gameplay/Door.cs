using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Door : ElectronicDevice
{
    public enum DoorState
    {
        Closed,
        Open
    }

    [System.Serializable]
    public struct DoorPanel
    {
        public GameObject panel;
        public Vector3 openPosition;
        public Vector3 closedPosition;
    }

    #region Door Mechanical Variables
    [Header("Door Variables")]
    [SerializeField] DoorState state;
    [SerializeField] AnimationCurve animationCurve;
    [SerializeField] [Min(0)] float transitionSpeed;
    [SerializeField] DoorPanel leftDoor,rightDoor;
    [SerializeField] bool locked;
    [SerializeField] Light light;

    float openPercentage;
    [SerializeField] BoxCollider detectionCollider;

    #endregion

    #region Door UI Variables
    [Header("UI Variables")]
    [SerializeField] Image lockedSymbol;
    [SerializeField] Color lockedColour, unlockedColour;

    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        resourcesDataPath = "Data/Door";
        base.Start();
    }

    private void Update()
    {
        CheckForCharacter();

        switch (state)
        {
            case DoorState.Closed:
                CloseDoor();
                break;
            case DoorState.Open:
                OpenDoor();
                break;
        }

    }

    public override void SetDeviceState(string _stateInfo)
    {
        base.SetDeviceState(_stateInfo);
        Debug.Log("State Info: " + _stateInfo);
        if (_stateInfo.Contains("Door UNLOCKED"))
            LockDoor(false);
        else if (_stateInfo.Contains("Door LOCKED"))
            LockDoor(true);

    }

    void OpenDoor()
    {
        //locked or already open
        if (locked || openPercentage >= 1)
            return;

        openPercentage += Time.deltaTime * transitionSpeed;

        MoveDoor(leftDoor, openPercentage);
        MoveDoor(rightDoor, openPercentage);
    }

    void CloseDoor()
    {
        //already closed
        if (openPercentage <= 0)
            return;

        openPercentage -= Time.deltaTime*transitionSpeed;

        MoveDoor(leftDoor, openPercentage);
        MoveDoor(rightDoor, openPercentage);
    }

    void MoveDoor(DoorPanel door, float _openPercentage)
    {
        openPercentage = Mathf.Clamp01(_openPercentage);

        //make the curve range between 0 and 1
        float openValue = animationCurve.Evaluate(openPercentage);
        door.panel.transform.localPosition = Vector3.Lerp(door.closedPosition, door.openPosition, openValue);
    }

    public void LockDoor(bool _locked)
    {
        locked = _locked;

        if (lockedSymbol != null)
        {
            if (locked)
            {
                lockedSymbol.color = lockedColour;
            }
            else
            {
                lockedSymbol.color = unlockedColour;
            }
        }

        if (light != null)
        {
            if (locked)
            {
                light.color = lockedColour;
            }
            else
            {
                light.color = unlockedColour;
            }
        }
    }

    public void SetDoorState(DoorState _state)
    {
        state = _state;
    }

    void CheckForCharacter()
    {
        if (detectionCollider == null)
        {
            Debug.LogError(this + " has no detection collider set.");
            return;
        }

        Collider[] overlappedColliders = Physics.OverlapBox(detectionCollider.transform.position + detectionCollider.center, detectionCollider.size);
        
        foreach (Collider col in overlappedColliders)
        {
            if (col.GetComponent<Character>() != null)
            {
                SetDoorState(DoorState.Open);
                return;
            }
        }
        SetDoorState(DoorState.Closed);
    }
}
