using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Guard : Character
{
    public enum GaurdState
    {
        Idle, //Remains still
        MirroredPatrol, //Navigates waypoints list from top to bottom, then bottom to top and repeats
        LoopingPatrol,  //Navigates waypoints list from top to bottom and repeats
        RandomPatrol, //Chooses from the set of waypoints to travel to at random
        Pursuit, //Detected Player and navigates to player location
        Guard //Detected Player and navigates to player location

    }

    NavMeshAgent agent;

    #region Guard Variables
    Vector3 guardPosition;
    #endregion
    #region Patrol Variables

    [Header("Patrol Variables")][Space]

    [SerializeField] [Min(0)] [Tooltip("Minimum time the agent must wait before conitinuing on path")] float minTimeToWait;
    [SerializeField] [Min(0)] [Tooltip("Maximum time the agent must wait before conitinuing on path")] float maxTimeToWait;
    [SerializeField] [Tooltip("y-axis corresponds to min and max from range 0 -> 1. E.g. if y = 0.5, min = 0, max = 2. wait time would be 1.")] AnimationCurve guardWaitTime;
    float patrolSpeed, alertSpeed, idleTime, timeToWait;
    bool waited;

    [SerializeField] float detectionRadius;

    [SerializeField] GaurdState currentState, previousState;
    #endregion

    #region Waypoint Management Variables
    [Header("Waypoint Management Variables")][Space]
    [SerializeField] Waypoints.WaypointManager waypointManager; //must be assigned in inspector
    List<Waypoints.Waypoint> waypoints = new List<Waypoints.Waypoint>();
    [SerializeField]int currentIndex = 0;
    #endregion

    #region Player Detection Variables
    [Header("Player Detection Variables")]
    [SerializeField] Transform head;
    Vector3 detectorOffset = new Vector3(0, 3, 0);
    Transform initialHeadTransform;
    PlayerDetector detector; 
    Vector3 leftMostHeadRotation, rightmostHeadRotation; //Euler;
    [SerializeField] float headMovementRange = 130f;
    [SerializeField] float viewAngle = 130f;
    [SerializeField][Min(.1f)] float lookAroundMinSpeed, lookAroundMaxSpeed;
    float lookAroundSpeed;
    float lookAroundTime;
    bool lookingAround;
    bool foundPlayer;
    
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();

        //set movement speeds
        patrolSpeed = agent.speed;
        alertSpeed = agent.speed * 2f;

        //Set guard movement to patrol
        SetMoveSpeed(patrolSpeed);

        if (waypointManager == null)        
            Debug.LogWarning("Waypoints have not been set for " + this.gameObject);                
        else
            waypoints = waypointManager.GetAllWaypoints();

        detector = new PlayerDetector();


        guardPosition = transform.position;

        if (head != null)
        {            
            //Set limits // X - axis enables side to side head movement in this scenario.
            rightmostHeadRotation = head.localEulerAngles + new Vector3(headMovementRange/2,0,0);
            leftMostHeadRotation = head.localEulerAngles - new Vector3(headMovementRange / 2,0, 0);
            lookAroundSpeed = Random.Range(lookAroundMinSpeed, lookAroundMaxSpeed);
            initialHeadTransform = head.transform;
        }
    }
    // Update is called once per frame
    void Update()
    {
        Animate();
        
        switch (currentState)
        {
            case GaurdState.Idle:
                Waiting();
                break;
            case GaurdState.LoopingPatrol:
                OnLoopingPatrol();
                break;
            case GaurdState.MirroredPatrol:
                OnMirroredPatrol();
                break;
            case GaurdState.RandomPatrol:
                OnRandomPatrol();
                break;
            case GaurdState.Guard:
                GuardStance();
                break;
        }

        //Look out for the player
        Transform player = detector.DetectPlayer(transform, detectorOffset, detectionRadius, viewAngle, this.name);

        //Found Player and in pursuit (END STATE)
        if (player != null && !foundPlayer)
        {
            foundPlayer = true;
            currentState = GaurdState.Pursuit;
            SetMoveSpeed(alertSpeed);
            GoToPosition(player.position);
        }
    }

    private void LateUpdate()
    {
        LookAround();
    }

    /// <summary>
    /// Set movement speed of the guard.
    /// </summary>
    /// <param name="_speed">New guard speed.</param>
    void SetMoveSpeed(float _speed)
    {
        if(agent!=null)
        {
            agent.speed = _speed;
        }
    }

    /// <summary>
    /// Instructs the agent to move to position if position is possible to move to.
    /// </summary>
    /// <param name="_position"></param>
    bool GoToPosition(Vector3 _position)
    {
        return agent.SetDestination(_position);
    }

    /// <summary>
    /// An agent behaviour: agent goes to waypoints in order and loops back to the first waypoint once reaching last
    /// </summary>
    void OnLoopingPatrol()
    {
        if (!HasWaypoints())
            return;

        if (ReachedDestination())
        {
            if (!waited)
                StartWaiting();
            else
            {
                currentIndex++;
                currentIndex = currentIndex % waypoints.Count;
                GoToPosition(waypoints[currentIndex].GetPosition());
                SetMoveSpeed(patrolSpeed);
                waited = false;
            }
        }
    }

    /// <summary>
    /// An agent behaviour: agent goes to waypoints in order and then goes to waypoints in reverse order once reaching last waypoint.
    /// Repeats after reaching first waypoint again.
    /// </summary>
    void OnMirroredPatrol()
    {
        if (!HasWaypoints())
            return;

        if (ReachedDestination())
        {
            if (!waited)
                StartWaiting();
            else
            {
                currentIndex++;
                if (currentIndex == ((waypoints.Count - 1) * 2)) // one index before returning to 0 index position (restart loop)
                    currentIndex = 0;

                int mirroredIndex = currentIndex; // save true index

                if (currentIndex >= waypoints.Count)
                    currentIndex = (waypoints.Count - 1) * 2 - mirroredIndex;

                GoToPosition(waypoints[currentIndex].GetPosition());
                currentIndex = mirroredIndex; //reset index to actual value
                SetMoveSpeed(patrolSpeed);
                waited = false;
            }
        }
    }

    /// <summary>
    /// An agent behaviour: agent goes to any of the waypoints available.
    /// </summary>
    void OnRandomPatrol()
    {
        if (!HasWaypoints())
            return;

        if (ReachedDestination())
        {
            if (!waited)
                StartWaiting();
            else
            {
                currentIndex = Random.Range(0, waypoints.Count);
                GoToPosition(waypoints[currentIndex].GetPosition());
                SetMoveSpeed(patrolSpeed);
                waited = false;
            }
        }
    }

    /// <summary>
    /// An agent behaviour: agent goes any of the waypoints available that isn't the same as the last if possible.
    /// </summary>
    public void Investigate(Vector3 _worldPosition)
    {
        GoToPosition(_worldPosition);
        SetMoveSpeed(alertSpeed);
    }



    /// <summary>
    /// Check if the agent has reached its destination.
    /// </summary>
    /// <returns></returns>
    bool ReachedDestination()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the agent has waypoints to travel to.
    /// </summary>
    /// <returns></returns>
    bool HasWaypoints()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning(this.gameObject + " has 0 waypoints. Cannot pursue " + currentState + " behaviour.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Finds the distance that the guard would have to travel to point on Navmesh.
    /// </summary>
    /// <param name="_worldPosition">point to travel to.</param>
    /// <returns>Distance the guard must travel.</returns>
    public float GetTravelDistanceToPoint(Vector3 _worldPosition)
    {
        NavMeshPath _path = new NavMeshPath();
        if (agent.CalculatePath(_worldPosition, _path))
        {
            // Create an array of points which is the length of the number of corners in the path + 2.
            Vector3[] waypoints = new Vector3[_path.corners.Length + 2];

            //starting point is the guard position
            waypoints[0] = transform.position;

            // The last point is destination point 
            waypoints[waypoints.Length - 1] = _worldPosition;

            // The points inbetween are the corners of the path.
            for (int i = 0; i < _path.corners.Length; i++)
            {
                waypoints[i + 1] = _path.corners[i];
            }

            float totalDistance = 0;

            // Add each segment of the path
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                totalDistance += Vector3.Distance(waypoints[i], waypoints[i + 1]);
            }

            return totalDistance;
        }


        return Mathf.Infinity;
    }


    void StartWaiting()
    {
        idleTime = 0;
        timeToWait = Mathf.Lerp(minTimeToWait,maxTimeToWait,guardWaitTime.Evaluate(Random.Range(0.0f, 1.0f)));
        previousState = currentState;
        currentState = GaurdState.Idle;
        lookingAround = true;
    }

    void Waiting()
    {
        idleTime += Time.deltaTime;
        if(idleTime >= timeToWait)
        {
            waited = true;
            currentState = previousState;
            previousState = GaurdState.Idle;
        }
    }
    /// <summary>
    /// An agent behaviour: remains stationary but responds to alerts.
    /// </summary>
    void GuardStance()
    {
        if (ReachedDestination())
        {
            if (!waited)
                StartWaiting();
            else
            {
                GoToPosition(guardPosition);
                waited = false;
            }
        }
    }

    protected override void Animate()
    {
        base.Animate();

        if (animator != null)
        {
            animator.SetFloat("moveSpeed", Mathf.Clamp01(agent.velocity.magnitude / alertSpeed)); //requires moveSpeed varirable in animator controller
        }
    }

    /// <summary>
    /// Applies a head movement to the player.
    /// </summary>
    void LookAround()
    {
        if (!lookingAround)
            return;

        if(head==null)
        {
            Debug.LogWarning("No head assigned to apply head movement to.");
            return;
        }
       

        lookAroundTime += Time.deltaTime;
        //Gets a sin value between -1 and 1
        float rotationLerp = Mathf.Sin(lookAroundTime * lookAroundSpeed);
        //make the curve range between 0 and 1
        rotationLerp = (rotationLerp + 1) / 2;

        head.transform.localEulerAngles = Vector3.Lerp(leftMostHeadRotation, rightmostHeadRotation, rotationLerp);

        if(lookAroundTime * lookAroundSpeed >= 2*Mathf.PI) //1 cycle
        {
            //reset head pos
            head.transform.localEulerAngles = Vector3.Lerp(leftMostHeadRotation, rightmostHeadRotation, .5f);
            lookingAround = false;
            lookAroundTime = 0;
            lookAroundSpeed = Random.Range(lookAroundMinSpeed, lookAroundMaxSpeed);
        }
    }   
}
