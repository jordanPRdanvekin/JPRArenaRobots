using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mira : MonoBehaviour
{
public GameObject target;
    Vector3 directionWanted;

    private void FixedUpdate()
    {
        if (target != null)
        {
            directionWanted = target.transform.position;
            directionWanted.y += 3f;
            transform.LookAt(directionWanted);
        }
    }
}
