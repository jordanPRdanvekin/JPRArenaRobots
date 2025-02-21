using UnityEngine;

/// <summary>
/// Movimiento oscilatorio que sigue una trayectoria en "∞" o esférica.
/// Retorna a su posición original y varía su velocidad aleatoriamente.
/// </summary>
public class LoopingReturnMovement : MonoBehaviour
{
    [SerializeField] private float minSpeed = 1f; // Velocidad mínima de avance
    [SerializeField] private float maxSpeed = 3f; // Velocidad máxima de avance
    [SerializeField] private float rotationSpeed = 1f;  // Velocidad de oscilación
    [SerializeField] private float rotationAmplitude = 30f; // Amplitud de oscilación en X e Y
    [SerializeField] private float cycleDuration = 5f; // Tiempo en segundos para completar un ciclo

    private Vector3 startPosition; // Posición inicial
    private float timeCounter = 0f; // Contador para la oscilación
    private float moveSpeed; // Velocidad de movimiento actual

    void Start()
    {
        startPosition = transform.position; // Guardar la posición inicial
        ChangeSpeed(); // Asignar una velocidad aleatoria al inicio
        InvokeRepeating(nameof(ChangeSpeed), 2f, 3f); // Cambia la velocidad cada 3 segundos
    }

    void Update()
    {
        timeCounter += Time.deltaTime * rotationSpeed;

        // Movimiento oscilatorio en X e Y basado en seno y coseno
        float xOffset = Mathf.Sin(timeCounter) * rotationAmplitude;
        float yOffset = Mathf.Cos(timeCounter) * rotationAmplitude;

        // Mueve el objeto con oscilación pero regresando a su posición inicial
        transform.position = startPosition + transform.forward * (moveSpeed * timeCounter) + new Vector3(xOffset, yOffset, 0);

        // Si el tiempo del ciclo se cumple, reseteamos
        if (timeCounter >= cycleDuration)
        {
            timeCounter = 0f;
            transform.position = startPosition; // Volver a la posición inicial
        }
    }

    /// <summary>
    /// Cambia aleatoriamente la velocidad dentro del rango definido.
    /// </summary>
    private void ChangeSpeed()
    {
        moveSpeed = Random.Range(minSpeed, maxSpeed);
    }
}

