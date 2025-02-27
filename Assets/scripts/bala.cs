using UnityEngine;

/// <summary>
/// Movimiento oscilatorio que sigue una trayectoria en "∞" o esférica.
/// Retorna a su posición original y varía su velocidad aleatoriamente.
/// este bucle sirve para poder experimentar y explorar el comportamiento del proyectil base 
/// </summary>
public class LoopingReturnMovement : MonoBehaviour
{
    [SerializeField] private float minVel = 1f; // Velocidad mínima de avance
    [SerializeField] private float maxVel = 3f; // Velocidad máxima de avance
    [SerializeField] private float rotarObj = 1f;  // Velocidad de oscilación
    [SerializeField] private float velRot = 30f; // Amplitud de oscilación en X e Y
    [SerializeField] private float bucleDurar = 5f; // Tiempo en segundos para completar un ciclo

    private Vector3 posiciOrigen; // Posición inicial
    private float temporizador = 0f; // Contador para la oscilación
    private float velocidadMov; // Velocidad de movimiento actual

    void Start()
    {
        posiciOrigen = transform.position; // Guardar la posición inicial
        ChangeSpeed(); // Asignar una velocidad aleatoria al inicio
        InvokeRepeating(nameof(ChangeSpeed), 2f, 3f); // Cambia la velocidad cada 3 segundos
    }

    void Update()
    {
        temporizador += Time.deltaTime * rotarObj;

        // Movimiento oscilatorio en X e Y basado en seno y coseno
        float distanciaX = Mathf.Sin(temporizador) * velRot;
        float distanciaY = Mathf.Cos(temporizador) * velRot;

        // Mueve el objeto con oscilación pero regresando a su posición inicial
        transform.position = posiciOrigen + transform.forward * (velocidadMov * temporizador) + new Vector3(distanciaX, distanciaY, 0);

        // Si el tiempo del ciclo se cumple, reseteamos
        if (temporizador >= bucleDurar)
        {
            temporizador = 0f;
            transform.position = posiciOrigen; // Volver a la posición inicial
        }
    }

    /// <summary>
    /// Cambia aleatoriamente la velocidad dentro del rango definido.
    /// </summary>
    private void ChangeSpeed()
    {
        velocidadMov = Random.Range(minVel, maxVel);
    }
}

