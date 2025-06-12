using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waypointViewer : MonoBehaviour
{
    [SerializeField]
    Color gizmosColor;

    [SerializeField]
    Vector3 gizmosSize;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmosColor;
        Gizmos.DrawCube(transform.position, gizmosSize);
    }
}
