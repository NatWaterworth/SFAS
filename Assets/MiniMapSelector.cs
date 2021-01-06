using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Game))]
public class MiniMapSelector : MonoBehaviour
{

    [SerializeField] Camera cam;
    [SerializeField] GameObject screen;
    [SerializeField] GameObject mouse;
    [SerializeField] Vector2 screenPosPercentage = Vector2.zero;
    [SerializeField] ElectronicDevice selectedObject;
    Vector2 mouseScreenPos;


    ElectronicDevice selectedDevice;
    string interactiveMessage;
    Game console;


    private void Start()
    {
        cam = GetComponent<Camera>();
        console = GetComponent<Game>();
    }

    private void Update()
    {
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

    void SetSecuirityDeviceState()
    {

        if (selectedDevice.Equals(typeof(SecuirityCamera)))
        {
            if (interactiveMessage.Contains("static"))
                ((SecuirityCamera)selectedDevice).SetCameraState(SecuirityCamera.CameraState.Static);
            else if (interactiveMessage.Contains("sentry mode"))
                ((SecuirityCamera)selectedDevice).SetCameraState(SecuirityCamera.CameraState.SentryMode);
        }
    }

    void RaycastToMap()
    {
        //RaycastToScreen();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {
            Vector3 localHit = screen.transform.InverseTransformPoint(hit.point);
            Vector2 screenPos = new Vector2(localHit.x, localHit.z);
            //based on plane so 5 is the half width.
            float width = Mathf.InverseLerp(5, -5, localHit.x);
            float height = Mathf.InverseLerp(5, -5, localHit.z);

            screenPosPercentage = new Vector2(width, height);
        }

    }

    void MoveMouseCursor()
    {
        //Selecting the point based on input position on current screen. Needs to be on texture.
        mouseScreenPos = new Vector2(screenPosPercentage.x * Screen.width, screenPosPercentage.y * Screen.height);

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

        if (Input.GetMouseButtonDown(0))
        {
            //Define Raycast information
            Ray ray = cam.ScreenPointToRay(mouseScreenPos);
            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Hackable");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
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
        
        selectedObject = _selectedObject.GetComponent<ElectronicDevice>();
        Debug.Log("Selected Object: " + _selectedObject);

        //Set console camera
        if (selectedDevice.HasDeviceCamera())
            console.SetConsoleCamera(selectedDevice.GetDeviceCamera());
        else
            console.SetDefaultConsoleCamera();

        //set console information
        if (selectedDevice.HasDeviceData())
            console.SetConsoleInformation(selectedDevice.GetDeviceData());
        else
            console.SetErrorInformation();

        
    }

    public ElectronicDevice GetSelectedDevice()
    {
        if (selectedObject!=null)
        {
            return selectedObject;
        }
        return null;
    }
}
