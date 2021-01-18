using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hacker : MonoBehaviour
{ 
    [SerializeField] Transform bodyPart;
    [SerializeField] [Tooltip("World position.")]Transform ObjectCentralStartingPoint;
    Vector3 mouseCentrePoint;
    Vector3 bodyPartStartingPoint;
    [SerializeField] [Tooltip("Local position of mouse.")]Vector2 minMousePosition, maxMousePosition;
    [SerializeField] MiniMapSelector mouseController;

    // Start is called before the first frame update
    void Start()
    {
        mouseCentrePoint = ObjectCentralStartingPoint.position;
        bodyPartStartingPoint = bodyPart.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// Apply animation updates here.
    /// </summary>
    private void LateUpdate()
    {
        if(mouseController!=null)
            UpdateArmPosition(mouseController.GetScreenPosition());
    }


    /// <summary>
    /// Update arm position based on where Vector2 percentagePos
    /// </summary>
    void UpdateArmPosition(Vector2 percentagePos)
    {
        float x = Mathf.Lerp(minMousePosition.x, maxMousePosition.x, percentagePos.x);
        float z = Mathf.Lerp(minMousePosition.y, maxMousePosition.y, percentagePos.y);

        bodyPart.position = bodyPartStartingPoint +  new Vector3(x, 0, z);
        ObjectCentralStartingPoint.position = mouseCentrePoint + new Vector3(x, 0, z);
    }
}
