using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyView : MonoBehaviour
{
    [SerializeField] private float rangoVision = 5f; // Puedes modificarlo desde el Inspector
    bool caught = false;

    private void FixedUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, rangoVision))
        {
            if (hit.collider.gameObject.CompareTag("Player") && !caught)
            {
                caught = true;
                ReiniciarJuego.instance.LostGame();
            }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * rangoVision, Color.yellow);
        }
    }

    private void OnDrawGizmos()
    {
        // Dibuja un c�rculo para visualizar el rango de visi�n
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(Vector3.forward) * rangoVision, 0.5f);
    }
}
