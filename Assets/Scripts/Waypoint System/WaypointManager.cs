using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Waypoints { 

    public class WaypointManager : MonoBehaviour
    {
        [HideInInspector][SerializeField] List<Waypoint> waypoints;  

        private void Awake()
        {
            if (waypoints != null)
            {
                foreach (Waypoint allPoints in waypoints)
                    allPoints.SetWaypointManager(this);
            }
        }

        /// <summary>
        /// Returns a list of waypoints managed by this Waypoint Manager.
        /// </summary>
        /// <returns></returns>
        public List<Waypoint> GetAllWaypoints()
        {
            //if there are no waypoints make a new list
            if (waypoints == null)
                waypoints = new List<Waypoint>();


            //Set all waypoints in list to be parented to the manager.
            foreach (Waypoint _waypoint in waypoints)
            {
                _waypoint.SetWaypointManager(this);
            }
            
            return waypoints;
        }

        /// <summary>
        /// Adds a waypoint to the Waypoint Manager list. If a list doesn't exist one is created.
        /// </summary>
        /// <param name="_waypoint"></param>
        /// <param name="_index"></param>
        public void AddWaypoint(Waypoint _waypoint, int _index = -1)
        {
            if (waypoints == null)
                waypoints = new List<Waypoint>();
            if (_index == -1)
                waypoints.Add(_waypoint);
            else
                waypoints.Insert(_index, _waypoint);
            _waypoint.SetWaypointManager(this);
        }

    }   
}
