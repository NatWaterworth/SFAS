using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

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
        public NavMeshObstacle navMeshObstacle;
    }

    #region Door Mechanical Variables
    [Header("Door Mechanics")]
    [SerializeField] DoorState state;
    [SerializeField] AnimationCurve animationCurve;
    [SerializeField] [Min(0)] float transitionSpeed;
    [SerializeField] DoorPanel leftDoor,rightDoor;
    [Header("Door Lock Visuals")]
    [SerializeField] bool locked;
    [SerializeField] Light frontIndicatorLight, backIndicatorLight;
    [SerializeField] MeshRenderer lockIndicatorFront, lockIndicatorBack;
    [SerializeField] Material lockedMaterial, unlockedMaterial;

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
        //Set door to how it is in the inspector.
        LockDoor(locked);
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

    public override void UpdateDeviceState(string _stateInfo)
    {
        base.UpdateDeviceState(_stateInfo);

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

        //Set whether AI can navigate through doors.

        if (leftDoor.navMeshObstacle != null)
        {
            leftDoor.navMeshObstacle.enabled = locked;
        }
        else
            Debug.LogError(this + " has not set navmeshobstacle for left door!");


        if (rightDoor.navMeshObstacle != null)
        {
            rightDoor.navMeshObstacle.enabled = locked;
        }
        else
            Debug.LogError(this + " has not set navmeshobstacle for right door!");


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
        else
        {
            Debug.LogWarning(this + " doesn't have an locked Symbol set!");
        }

        if (frontIndicatorLight != null && backIndicatorLight != null)
        {
            if (locked)
            {
                frontIndicatorLight.color = lockedColour;
                backIndicatorLight.color = lockedColour;
            }
            else
            {
                frontIndicatorLight.color = unlockedColour;
                backIndicatorLight.color = unlockedColour;
            }
        }
        else
        {
            Debug.LogWarning(this + " doesn't have an indicator light set!");
        }

        if (lockIndicatorFront != null && lockIndicatorBack != null && lockedMaterial != null && unlockedMaterial != null)
        {
            if (locked)
            {
                lockIndicatorFront.material = lockedMaterial;
                lockIndicatorBack.material = lockedMaterial;
            }
            else
            {
                lockIndicatorFront.material = unlockedMaterial;
                lockIndicatorBack.material = unlockedMaterial;
            }
        }
        else
        {
            Debug.LogWarning(this + " doesn't have an indicator variable set!");
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
