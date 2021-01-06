using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Waypoints
{
    /// <summary>
    /// Waypoint class  - managed through the WaypointGroups editor
    /// 
    /// </summary>
    [System.Serializable]
    public class Waypoint
    {
        Vector3 position;

        [HideInInspector]
        Quaternion rotation = Quaternion.identity;

        [SerializeField]
        [HideInInspector]
        Vector3 offsetFromParentPosition;   

        WaypointManager manager;


        public Vector3 XYZ
        {
            get { return offsetFromParentPosition; }
        }

        public Quaternion GetRotation()
        {
            return rotation;
        }


        public void SetWaypointManager(WaypointManager _manager)
        {
            manager = _manager;
        }
    
        public Vector3 GetPosition()
        {
            position = offsetFromParentPosition;

            if (manager != null)
                return manager.transform.position + position;
            else
                return position;
        }

        public void UpdatePosition(Vector3 _position)
        {
            offsetFromParentPosition.z += _position.z;
            offsetFromParentPosition.y += _position.y;
            offsetFromParentPosition.x += _position.x;
        }

        public void Copy(Waypoint _waypoint)
        {
            if (_waypoint == null) return;

            offsetFromParentPosition = _waypoint.XYZ;
        }

    }
}
