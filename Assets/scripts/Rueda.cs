using UnityEngine;
using LeanTween;

/// <summary>
/// Controla el movimiento de la rueda, que act�a como el jugador.
/// </summary>
public class Rueda : MonoBehaviour
{
    [SerializeField] private float fuerzaMovimiento = 5f; // Velocidad de avance
    [SerializeField] private float velocidadGiro = 100f; // Velocidad de giro
    [SerializeField] private float suavizadoAceleracion = 2f; // Suavidad en aceleraci�n/desaceleraci�n
    private float velocidadActual = 0f;

    void Update()
    {
        // Girar la rueda en el eje Y con A y D
        float giro = Input.GetAxis("Horizontal") * velocidadGiro * Time.deltaTime;
        transform.Rotate(0, giro, 0);

        // Suavizar la aceleraci�n y desaceleraci�n en el eje Z (avanzar/retroceder)
        float objetivoVelocidad = Input.GetAxis("Vertical") * fuerzaMovimiento;
        velocidadActual = Mathf.Lerp(velocidadActual, objetivoVelocidad, Time.deltaTime * suavizadoAceleracion);
    }

    void FixedUpdate()
    {
        // Mover la rueda hacia adelante o atr�s en su direcci�n actual
        transform.position += transform.forward * velocidadActual * Time.fixedDeltaTime;

        // Rotar la rueda en el eje X para simular movimiento realista
        transform.Rotate(Vector3.right * velocidadActual * 10 * Time.fixedDeltaTime);
    }
}






