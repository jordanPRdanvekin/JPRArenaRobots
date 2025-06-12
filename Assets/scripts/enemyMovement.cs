using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyMovement : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    [SerializeField] Transform[] waypoints;

    int currentWaypoint;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0)
        {
            navMeshAgent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    void Update()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                GoToNextWaypoint();
            }
    }

    void GoToNextWaypoint()
    {
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        navMeshAgent.SetDestination(waypoints[currentWaypoint].position);
    }
}
