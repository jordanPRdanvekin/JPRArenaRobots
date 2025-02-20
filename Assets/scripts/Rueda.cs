using UnityEngine;

public class WheelController : MonoBehaviour
{
    // Fuerza máxima de impulso para avanzar o retroceder (valor objetivo 2)
    public float maxImpulse = 2f;
    // Velocidad a la que el impulso aumenta al presionar W o S
    public float accelerationRate = 2f;
    // Velocidad a la que el impulso disminuye al dejar de presionar
    public float decelerationRate = 2f;
    // Velocidad de giro para la rotación en Y (para elegir izquierda o derecha)
    public float turnSpeed = 100f;
    // Velocidad máxima permitida para evitar que la rueda se descontrole
    public float maxSpeed = 10f;

    // Impulso actual que se irá ajustando (de -2 a 2)
    private float currentImpulse = 0f;
    // Referencia al Rigidbody de la rueda
    private Rigidbody rb;

    void Start()
    {
        // Se obtiene el componente Rigidbody (asegúrate de que el objeto lo tenga)
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // GESTIÓN DEL GIRO:
        // Se usan las teclas A y D para rotar la rueda en el eje Y (girando hacia izquierda o derecha).
        float turnInput = Input.GetAxis("Horizontal");  // -1 (A) a 1 (D)
        transform.Rotate(0, turnInput * turnSpeed * Time.deltaTime, 0);

        // GESTIÓN DEL IMPULSO:
        // Se usan W y S para mover la rueda hacia adelante o atrás, considerando que
        // el eje X local es el "forward" de la rueda.
        float moveInput = Input.GetAxisRaw("Vertical"); // 1 (W) o -1 (S)

        if (moveInput != 0)
        {
            // Cuando se presiona W o S, el impulso aumenta gradualmente de 0 hasta maxImpulse (o -maxImpulse)
            currentImpulse = Mathf.MoveTowards(currentImpulse, maxImpulse * moveInput, accelerationRate * Time.deltaTime);
        }
        else
        {
            // Si no se presiona nada, el impulso disminuye gradualmente hasta 0
            currentImpulse = Mathf.MoveTowards(currentImpulse, 0, decelerationRate * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // Se aplica una fuerza continua a lo largo del eje X local para mover la rueda
        rb.AddRelativeForce(Vector3.right * currentImpulse, ForceMode.Force);

        // Se limita la velocidad del Rigidbody para evitar movimientos excesivos o salir del mapa
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        // SIMULACIÓN DEL ROLLO DE LA RUEDA:
        // La rueda rota alrededor de su eje Z para simular el efecto de rodar conforme se aplica el impulso.
        // Se utiliza un factor (aquí 360) para ajustar visualmente la velocidad de giro; se rota en sentido inverso.
        float rollAmount = currentImpulse * 360f * Time.fixedDeltaTime;
        transform.Rotate(0, 0, -rollAmount);
    }
}


