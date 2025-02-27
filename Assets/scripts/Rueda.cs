using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Controla el movimiento de la rueda, que actúa como el jugador.
/// es el cuerpo real del jugador por lo que usa su rigidbody 
/// la idea es un movimiento similar al de una moto.
/// </summary>
public class Rueda : MonoBehaviour
{
    [SerializeField] private float fuerzaMov = 1f; // Velocidad de avance
    [SerializeField] private float rotVel = 100f; // Velocidad de giro
    [SerializeField] private float velAct = 1f;
    [SerializeField] Rigidbody rbrueda;

    private void Start()
    {
        //se obtiene el ''cuerpo'' de la rueda
        rbrueda = GetComponent<Rigidbody>();
    }
    void Update()
    {
        // Girar la rueda en el eje Y con A y D
        float giro = Input.GetAxis("Horizontal") * rotVel * Time.deltaTime;
        transform.Rotate(0, giro, 0);
    }

    void FixedUpdate()
    {

        // Mover la rueda hacia adelante o atrás en su dirección actual manteniendo una velocidad constante
        if (Input.GetKey(KeyCode.W))
        {
            velAct += fuerzaMov;
            rbrueda.AddForce(transform.forward * velAct);
            if (velAct > 5f)
            {
                velAct = velAct - 1f;
            }
        }
       
        if (Input.GetKey(KeyCode.S))
        {
            velAct -= fuerzaMov;
            rbrueda.AddForce(transform.forward * velAct);
            if (velAct < -5f)
            {
                velAct = velAct + 1f;
            }
        }
        
        if (velAct == 0f)
        {
            transform.Rotate(0, 0, 0);
        }
       
        // Rotar la rueda en el eje X para simular movimiento realista
        transform.Rotate(Vector3.right * velAct * 10 * Time.fixedDeltaTime);
    }
}






