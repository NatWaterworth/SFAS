using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
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

    float openPercentage;
    BoxCollider collider;

    const string resourcesDataPath = "Data/Door";
    StoryData data;
    #endregion

    #region Door UI Variables
    [Header("UI Variables")]
    [SerializeField] Image lockedSymbol;

    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        collider = GetComponent<BoxCollider>();
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
    }

    public void SetDoorState(DoorState _state)
    {
        state = _state;
    }

    void CheckForCharacter()
    {
        Collider[] overlappedColliders = Physics.OverlapBox(transform.position + collider.center, collider.size);
        
        //Debug.Log(overlappedColliders.Length);
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
