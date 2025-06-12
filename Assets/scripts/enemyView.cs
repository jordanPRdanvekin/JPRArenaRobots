using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyView : MonoBehaviour
{
    bool caught = false;

    private void FixedUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.CompareTag("Player") && !caught)
            {
                caught = true;
                ReiniciarJuego.instance.LostGame();
            }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        }
    }
}
