using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Waypoints
{

    [CustomEditor(typeof(WaypointManager))]
    public class WaypointEditor : Editor
    {
        WaypointManager waypointManager;
        List<Waypoint> waypoints;

        Waypoint selectedWaypoint = null;



        private void OnEnable()
        {
            waypointManager = target as WaypointManager;
            waypoints = waypointManager.GetAllWaypoints();
        }


        private void OnSceneGUI()
        {
            DrawAllWaypoints(waypoints);
        }


        /// <summary>
        ///  A menu option to create a waypoint manager
        /// </summary>
        [MenuItem("Waypoints/Create Waypoint Set")]
        public static void CreateWaypointSet()
        {
            //Create a waypoint manager
            GameObject waypointManager = new GameObject("WaypointSet");
            waypointManager.AddComponent<WaypointManager>();

            //Add it to a gameobject that holds all waypoint managers
            GameObject parent = GameObject.Find("WaypointSets");

            if (parent == null)
            {
                parent = new GameObject("WaypointSets");
            }

            waypointManager.transform.parent = parent.transform;

            //Have it selected in editor
            Selection.activeGameObject = waypointManager;
        }

        /// <summary>
        /// Updates the WaypointManager component when the user interacts with the Unity Inspector
        /// </summary>
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Waypoints", EditorStyles.boldLabel, GUILayout.MinWidth(60));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Index", EditorStyles.miniBoldLabel, GUILayout.MinWidth(40));
            EditorGUILayout.LabelField("Position", EditorStyles.miniBoldLabel, GUILayout.MinWidth(60));
            EditorGUILayout.LabelField("Options", EditorStyles.miniBoldLabel, GUILayout.MinWidth(10));
            EditorGUILayout.EndHorizontal();
            bool updateGizmos = false;

            Color guiColor = GUI.color; //Save GUI colour for later reassignment

            if (waypoints != null)
            {
                int delIndex = -1;

                for (int count = 0; count < waypoints.Count; count++)
                {


                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal(); //make the waypoint and info appear on 1 line
                    EditorGUILayout.LabelField(count.ToString(), EditorStyles.boldLabel,GUILayout.MaxWidth(30));

                    

                    Waypoint currentWaypoint = waypoints[count];

                    //change colour if selected
                    if (currentWaypoint == selectedWaypoint)
                        GUI.color = new Color(.5f, .7f, .5f, .8f);
                    else
                        GUI.color = guiColor;

                    //positional information              
                    EditorGUI.BeginChangeCheck();
                    Vector3 previousPosition = currentWaypoint.GetPosition();

                    Vector3 newPosition = EditorGUILayout.Vector3Field("", previousPosition);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(waypointManager, "Altered Waypoint");
                        currentWaypoint.UpdatePosition(newPosition - previousPosition);
                    }

                    //Highlighted button
                    if (currentWaypoint == selectedWaypoint)
                        GUI.color = new Color(.5f, .9f, .5f, .8f);
                    else
                        GUI.color = new Color(.5f, .7f, .5f, .8f);

                    if (GUILayout.Button("Select", GUILayout.MinWidth(5)))
                    {
                        if (selectedWaypoint == currentWaypoint)
                        {
                            selectedWaypoint = null;
                        }
                        else
                        {
                            selectedWaypoint = currentWaypoint;
                        }

                        updateGizmos = true;

                    }

                    //Remove button
                    GUI.color = new Color(.7f, .5f, .5f, .8f);

                    if (GUILayout.Button("Remove", GUILayout.MinWidth(5)))
                    {
                        delIndex = count;
                        updateGizmos = true;

                    }

                    
                    EditorGUILayout.EndHorizontal();

                }

                if (delIndex > -1)
                {
                    if (waypoints[delIndex] == selectedWaypoint)
                        selectedWaypoint = null;
                    waypoints.RemoveAt(delIndex);
                }

            }

            EditorGUILayout.Space();
            GUI.color = new Color(.5f, .5f, .7f, .8f);

            if (GUILayout.Button("Add Waypoint"))
            {
                Undo.RecordObject(waypointManager, "Altered Waypoint");
                int index = -1;
                if (selectedWaypoint != null)
                {
                    index = waypoints.IndexOf(selectedWaypoint);
                    if (index == -1)
                        selectedWaypoint = null;
                    else
                        index += 1;
                }


                Waypoint waypoint = new Waypoint();
                waypoint.Copy(selectedWaypoint);
                waypointManager.AddWaypoint(waypoint, index);
                selectedWaypoint = waypoint;
                updateGizmos = true;

            }

            GUI.color = guiColor;  //reassign GUI colour

            EditorGUILayout.EndVertical();
            if (updateGizmos)
            {
                SceneView.RepaintAll();
            }

        }

        public void DrawAllWaypoints(List<Waypoint> _waypoints)
        {
            bool updateGizmos = false;

            if (_waypoints != null)
            {

                int count = 0;
                foreach (Waypoint waypoint in _waypoints)
                {
                    updateGizmos |= DrawWaypoint(waypoint, count);

                    // Draw a pointer line 
                    if (count < _waypoints.Count - 1)
                    {
                        Handles.color = Color.red;
                        Waypoint nextWaypoint = _waypoints[count + 1];
                        Handles.DrawLine(waypoint.GetPosition(), nextWaypoint.GetPosition());
                    }
                    else
                    {
                        Waypoint nextWaypoint = _waypoints[0];
                        Color colour = Handles.color;
                        Handles.color = Color.yellow;
                        Handles.DrawLine(waypoint.GetPosition(), nextWaypoint.GetPosition());
                        Handles.color = colour;
                    }
                    count ++;
                }
            }
            if (updateGizmos)
            {
                Repaint();
            }

        }

        /// <summary>
        /// Draws a Waypoint Gizmo in the scene for easy manipulation
        /// </summary>
        /// <param name="_waypoint"></param>
        /// <returns></returns>
        public bool DrawWaypoint(Waypoint _waypoint, int _index)
        {
            if (_waypoint == null)
            {
                Debug.Log("No Waypoint!");
                return false;
            }

            bool updateGizmos = false;
            //None serialized field, gets "lost" during serailize updates;
            _waypoint.SetWaypointManager(waypointManager);

            //initialise style for latter use when labelling.
            GUIStyle style = new GUIStyle();

            if (selectedWaypoint == _waypoint)
            {
                //Apply colours to Gizmos
                Color guiColour = Color.green;
                Handles.color = guiColour;
                style.normal.textColor = guiColour;
                style.fontSize = 14;

                //Apply positional change to waypoint in scene to waypoint in list
                EditorGUI.BeginChangeCheck();
                Vector3 previousPosition = _waypoint.GetPosition();
                Vector3 newPosition = Handles.PositionHandle(previousPosition, _waypoint.GetRotation()); //sets waypoint new position based on scene handle

                float handleSize = HandleUtility.GetHandleSize(newPosition);

                Handles.ConeHandleCap(-1, newPosition, Quaternion.Euler(new Vector3(-90,0, 0)), 0.3f * handleSize, EventType.Repaint);
               
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(waypointManager, "Altered Waypoint");
                    _waypoint.UpdatePosition(newPosition - previousPosition);
                }
            }
            else
            {
                //Apply colours to Gizmos
                Color guiColour = new Color(1f, .5f, .5f);
                Handles.color = guiColour;
                style.normal.textColor = guiColour;
                style.fontSize = 13;

                Vector3 waypointPosition = _waypoint.GetPosition();
                float handleSize = HandleUtility.GetHandleSize(waypointPosition);
                if (Handles.Button(waypointPosition, Quaternion.Euler(new Vector3(-90, 0, 0)), 0.2f * handleSize, 0.2f * handleSize, Handles.ConeHandleCap))
                {
                    updateGizmos = true;
                    selectedWaypoint = _waypoint;
                }            
            }

            //annotate waypoint with count
            Handles.Label(_waypoint.GetPosition() + Vector3.up, "Waypoint: " + _index.ToString(), style);

            return updateGizmos;
        }

    }
}

