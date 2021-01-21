using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Game))]
public class MiniMapSelector : MonoBehaviour
{
    [SerializeField] bool takeInput;

    [Header("Screen")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject screen;

    [Header("Mouse")]
    [SerializeField] GameObject mouse;
    [SerializeField] Image activeMouseImage;
    [SerializeField] Sprite  defaultMouseCursor, highlightingMouseCursor;
    [SerializeField] Vector3 defaultCursorOffset, highlightedCursorOffset;
    [SerializeField] Color defaultCursorColour, highlightedCursorColour;
    [SerializeField] Vector2 screenPosPercentage = Vector2.zero;

    [Header("Hacked Device")]
    [SerializeField] ElectronicDevice selectedDevice;
    Vector2 mouseScreenPos;

    Game console;


    private void Start()
    {
        cam = GetComponent<Camera>();
        console = GetComponent<Game>();
    }

    private void Update()
    {
        if (!takeInput)
            return;

        RaycastToMap();
        MoveMouseCursor();
        SelectHackableDevice();

        SetDeviceState();
    }

    public void SetDeviceState()
    {
        if (selectedDevice == null)
            return;

        SetSecuirityDeviceState();
    }
    /// <summary>
    /// Set whether player mouse input is used to update MiniMapSelector.
    /// </summary>
    /// <param name="_takeInput">Set to True if you want MiniMapSelector to function.</param>
    public void SetTakingInput(bool _takeInput)
    {
        takeInput = _takeInput;
    }

    /// <summary>
    /// Return bool to signify if MiniMapSelector is being updated.
    /// </summary>
    public bool IsTakingInput()
    {
        return takeInput;
    }

    /// <summary>
    /// Set Mouse cursor to highlight mode. Focusing on any detectected hackable object.
    /// </summary>
    /// <param name="_highlight"></param>
    void MouseHighlight(bool _highlight)
    {
        if (_highlight)
        { 
            //set mouse Image to highlight
            activeMouseImage.sprite = highlightingMouseCursor;
            activeMouseImage.rectTransform.localPosition = highlightedCursorOffset;
            activeMouseImage.color = highlightedCursorColour;
        }
        else
        {
            //set mouse Image to default
            activeMouseImage.sprite = defaultMouseCursor;
            activeMouseImage.rectTransform.localPosition = defaultCursorOffset;
            activeMouseImage.color = defaultCursorColour;
        }
        
    }

    void SetSecuirityDeviceState()
    {
        if (selectedDevice != null)
            selectedDevice.SetDeviceState(console.GetCurrentBeatText());
    }

    void RaycastToMap()
    {
        //RaycastToScreen();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {      
            Vector3 localHit = screen.transform.InverseTransformPoint(hit.point);
            //based on plane so 5 is the half width.
            float width = Mathf.InverseLerp(5, -5, localHit.x);
            float height = Mathf.InverseLerp(5, -5, localHit.z);
            screenPosPercentage = new Vector2(width, height);
        }

    }
    /// <summary>
    /// Returns position as a percentage between min and max screen points
    /// </summary>
    public Vector2 GetScreenPosition()
    {
        return screenPosPercentage;
    }

    void MoveMouseCursor()
    {
        //Selecting the point based on input position on current screen. Needs to be on texture.
        mouseScreenPos = new Vector2(screenPosPercentage.x * Screen.width, screenPosPercentage.y * Screen.height);

        MouseHighlight(false);

        //Define Raycast information
        Ray ray = cam.ScreenPointToRay(mouseScreenPos);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
            mouse.transform.position = new Vector3(hit.point.x, mouse.transform.position.y, hit.point.z);
        }
    }
    
    void SelectHackableDevice()
    {     
        //Define Raycast information
        Ray ray = cam.ScreenPointToRay(mouseScreenPos);
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Hackable");
            
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            MouseHighlight(true);

            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

            if (Input.GetMouseButtonDown(0))
            {

                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlaySoundEffect("Select");
                }

                Debug.Log("Get hackable object: " + hit.collider.gameObject.name);
                SetSelectedDevice(hit.collider.gameObject);
            }
        }
        
    }

    void SetSelectedDevice(GameObject _selectedObject)
    {
        if (_selectedObject == null)
            return;

        Debug.Log("Selected Object: " + _selectedObject);
        if (_selectedObject.GetComponent<ElectronicDevice>() == null)
            return;

        //Check if null, may not have previously been set.
        if (selectedDevice != null)
        {
            //Deactivate previous camera, if there was one.
            if (selectedDevice.HasDeviceCamera())
                selectedDevice.SetCameraActive(false);
            else
                console.SetDefaultConsoleCameraActivity(false);
        }

        selectedDevice = _selectedObject.GetComponent<ElectronicDevice>();
        Debug.Log(selectedDevice + " camera: " + selectedDevice.HasDeviceCamera());

        //Set console camera
        if (selectedDevice.HasDeviceCamera())
        {
            console.SetConsoleCamera(selectedDevice.GetDeviceCamera());
            selectedDevice.SetCameraActive(true);
        }
        else
        {
            console.SetDefaultConsoleCamera();
            console.SetDefaultConsoleCameraActivity(true);
        }

        Debug.Log(selectedDevice + " data: " + selectedDevice.HasDeviceData());

        //set console information
        if (selectedDevice.HasDeviceData())
            console.SetConsoleInformation(selectedDevice.GetDeviceData());
        else
            console.SetErrorInformation();

        
    }

    public ElectronicDevice GetSelectedDevice()
    {
        if (selectedDevice!=null)
        {
            return selectedDevice;
        }
        return null;
    }
}
