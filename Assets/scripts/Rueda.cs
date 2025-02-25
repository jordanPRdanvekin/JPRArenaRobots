using UnityEngine;

/// <summary>
/// Controla el movimiento y la rotaci�n de la rueda con f�sicas de manera simple.
/// </summary>
public class Rueda : MonoBehaviour
{
    [SerializeField] private float fuerzaMovimiento = 3f;
    [SerializeField] private float velocidadGiro = 100f;
    [SerializeField] private float suavizadoAceleracion = 2f;
    private float velocidadActual = 0f;
    private float direccionActual = 0f;

    void Update()
    {
        // Girar la rueda en el eje Y con A y D
        float giro = Input.GetAxis("Horizontal") * velocidadGiro * Time.deltaTime;
        transform.Rotate(0, giro, 0);

        // Suavizar la aceleraci�n y desaceleraci�n
        float objetivoVelocidad = Input.GetAxis("Vertical") * fuerzaMovimiento;
        velocidadActual = Mathf.Lerp(velocidadActual, objetivoVelocidad, Time.deltaTime * suavizadoAceleracion);
    }

    void FixedUpdate()
    {
        // Mover la rueda hacia adelante o atr�s
        transform.position += transform.forward * velocidadActual * Time.fixedDeltaTime;

        // Simular rotaci�n de la rueda en el eje X al moverse
        transform.Rotate(Vector3.right * velocidadActual * 10 * Time.fixedDeltaTime);
    }
}






