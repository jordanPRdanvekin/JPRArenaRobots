using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Necesario para Coroutines

public class EnemyBehavior : MonoBehaviour
{
    [Header("Referencias Principales")]
    public NavMeshAgent agent;
    public Transform player;
    // public GameObject gameOverCanvas; // <-- YA NO NECESITAMOS ESTO. Puedes borrar la línea.
    private GameController gameController; // <-- AÑADIR ESTA LÍNEA

    // ... (El resto de tus variables de estado y patrulla permanecen igual) ...
    private enum State { Patrullando, Descansando, Persiguiendo, Buscando }
    private State currentState;
    [Header("Configuración de Patrulla")]
    public float radioDePatrulla = 20f;
    public float tiempoDeMovimiento = 10f;
    public float tiempoDeDescanso = 3f;
    private Vector3 puntoDeDestino;
    private bool destinoFijado;
    private float temporizadorDeEstado;
    [Header("Configuración de Retorno al Origen")]
    public float tiempoParaRegresar = 50f;
    private Vector3 posicionOriginal;
    private float temporizadorDeRegreso;
    [Header("Configuración de Detección")]
    public float radioDeEscucha = 3f;
    public float radioDeVision = 15f;
    [Range(0, 360)]
    public float anguloDeVision = 90f;
    public LayerMask capaDelJugador;
    public LayerMask capaDeObstaculos;
    private bool puedeVerAlJugador;
    private Vector3 ultimaPosicionConocidaDelJugador;
    [Header("Configuración de Persecución")]
    public float tiempoDeBusqueda = 20f;

    [Header("Configuración de Impacto del Jugador")]
    public float dispersionForce = 500f; // Fuerza para dispersar las partes del jugador
    public float rotationForce = 100f; // Fuerza de rotación para las partes del jugador
    public float dispersionDuration = 1.5f; // Tiempo que las partes están en dispersión antes de congelarse


    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0f;

        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Buscamos el GameController en la escena al iniciar.
        gameController = FindObjectOfType<GameController>(); // <-- AÑADIR ESTA LÍNEA
        if (gameController == null) // <-- AÑADIR ESTA COMPROBACIÓN
        {
            Debug.LogError("¡ERROR! No se encuentra un GameController en la escena. El enemigo no puede reportar la derrota.");
            this.enabled = false;
        }

        posicionOriginal = transform.position;
        temporizadorDeRegreso = tiempoParaRegresar;
        currentState = State.Patrullando;
        temporizadorDeEstado = tiempoDeMovimiento;
    }

    // ... (Tu Update y todas las funciones de la máquina de estados como ManejarEstados, Patrullar, etc. NO necesitan cambios) ...
    void Update()
    {
        ComprobarCampoDeVision();
        ManejarEstados();
    }
    private void ManejarEstados()
    {
        if (currentState != State.Persiguiendo && currentState != State.Buscando)
        {
            temporizadorDeRegreso -= Time.deltaTime;
            if (temporizadorDeRegreso <= 0f)
            {
                currentState = State.Patrullando;
                agent.SetDestination(posicionOriginal);
                destinoFijado = true;
                temporizadorDeRegreso = tiempoParaRegresar;
                temporizadorDeEstado = tiempoDeMovimiento;
                return;
            }
        }
        if (puedeVerAlJugador)
        {
            if (currentState != State.Persiguiendo)
            {
                currentState = State.Persiguiendo;
                temporizadorDeEstado = tiempoDeBusqueda;
                temporizadorDeRegreso = tiempoParaRegresar;
            }
        }
        else
        {
            if (currentState == State.Persiguiendo)
            {
                currentState = State.Buscando;
            }
        }
        switch (currentState)
        {
            case State.Patrullando: Patrullar(); break;
            case State.Descansando: Descansar(); break;
            case State.Persiguiendo: Perseguir(); break;
            case State.Buscando: Buscar(); break;
        }
    }
    private void Patrullar()
    {
        if (!destinoFijado)
        {
            BuscarNuevoPuntoDePatrulla();
        }
        if (agent.hasPath && agent.remainingDistance < 2f)
        {
            destinoFijado = false;
        }
        temporizadorDeEstado -= Time.deltaTime;
        if (temporizadorDeEstado <= 0)
        {
            currentState = State.Descansando;
            temporizadorDeEstado = tiempoDeDescanso;
            agent.ResetPath();
        }
    }
    private void Descansar()
    {
        temporizadorDeEstado -= Time.deltaTime;
        if (temporizadorDeEstado <= 0)
        {
            currentState = State.Patrullando;
            temporizadorDeEstado = tiempoDeMovimiento;
            destinoFijado = false;
        }
    }
    private void Perseguir()
    {
        agent.SetDestination(player.position);
        ultimaPosicionConocidaDelJugador = player.position;
    }
    private void Buscar()
    {
        agent.SetDestination(ultimaPosicionConocidaDelJugador);
        temporizadorDeEstado -= Time.deltaTime;
        if (temporizadorDeEstado <= 0 || (agent.hasPath && agent.remainingDistance < 1f))
        {
            currentState = State.Patrullando;
            temporizadorDeEstado = tiempoDeMovimiento;
            destinoFijado = false;
        }
    }
    private void BuscarNuevoPuntoDePatrulla()
    {
        Vector3 puntoAleatorio = Random.insideUnitSphere * radioDePatrulla;
        puntoAleatorio += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(puntoAleatorio, out hit, radioDePatrulla, NavMesh.AllAreas))
        {
            puntoDeDestino = hit.position;
            agent.SetDestination(puntoDeDestino);
            destinoFijado = true;
        }
    }
    private void ComprobarCampoDeVision()
    {
        Collider[] collidersEnEscucha = Physics.OverlapSphere(transform.position, radioDeEscucha, capaDelJugador);
        if (collidersEnEscucha.Length > 0)
        {
            puedeVerAlJugador = true;
            return;
        }
        Collider[] collidersEnRango = Physics.OverlapSphere(transform.position, radioDeVision, capaDelJugador);
        if (collidersEnRango.Length > 0)
        {
            Transform objetivo = collidersEnRango[0].transform;
            Vector3 direccionHaciaObjetivo = (objetivo.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, direccionHaciaObjetivo) < anguloDeVision / 2)
            {
                float distanciaHaciaObjetivo = Vector3.Distance(transform.position, objetivo.position);
                if (!Physics.Raycast(transform.position, direccionHaciaObjetivo, distanciaHaciaObjetivo, capaDeObstaculos))
                {
                    puedeVerAlJugador = true;
                    return;
                }
            }
        }
        puedeVerAlJugador = false;
    }


    /// <summary>
    /// Se activa cuando el enemigo toca al jugador. Notifica al GameController y dispersa los hijos del jugador.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Asegurarse de que el objeto que colisionó es el jugador principal (o cualquier parte del jugador con la etiqueta "Player")
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡El enemigo ha tocado al jugador! Dispersando las partes del jugador y notificando al GameController.");

            // Desactivamos al enemigo para que no pueda provocar múltiples Game Overs.
            // Esto desactiva su IA y su capacidad de moverse.
            this.enabled = false;
            agent.enabled = false;

            // Iniciar la corrutina para dispersar y congelar al jugador, y luego terminar el juego.
            StartCoroutine(DisperseAndFreezePlayer());
        }
    }

    /// <summary>
    /// Coroutine para dispersar las partes del jugador, desvincularlas, congelarlas y luego terminar el juego.
    /// </summary>
    IEnumerator DisperseAndFreezePlayer()
    {
        // Encontrar el Rigidbody del jugador principal (si lo tiene) y sus hijos con la etiqueta "Player"
        // Asumimos que 'player' ya apunta a la raíz del GameObject del jugador.
        Rigidbody[] playerPartsRbs = player.GetComponentsInChildren<Rigidbody>();

        // Primero, aplicar la fuerza de dispersión y desvincular
        foreach (Rigidbody rb in playerPartsRbs)
        {
            // Solo dispersar si el objeto tiene la etiqueta "Player"
            if (rb.gameObject.CompareTag("Player"))
            {
                // Asegurarse de que el Rigidbody está activo y no es cinemático para poder aplicar fuerzas
                rb.isKinematic = false;
                rb.useGravity = true; // Asegurarse de que la gravedad esté activada inicialmente para la dispersión

                // Calcular una dirección aleatoria de dispersión
                Vector3 randomDirection = Random.onUnitSphere;
                // Asegura que la fuerza tenga un componente hacia arriba para dar un efecto de "salto"
                randomDirection.y = Mathf.Abs(randomDirection.y) + 0.5f; // Añade un offset para asegurar movimiento hacia arriba
                randomDirection.Normalize(); // Normalizar la dirección para que la fuerza sea consistente

                rb.AddForce(randomDirection * dispersionForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * rotationForce, ForceMode.Impulse); // Añadir algo de rotación aleatoria

                // ¡Importante! Desvincular la parte del jugador de su padre
                rb.transform.parent = null;
            }
        }

        // Esperar el tiempo de dispersión
        yield return new WaitForSeconds(dispersionDuration);

        // Después del tiempo de dispersión, congelar las partes
        foreach (Rigidbody rb in playerPartsRbs)
        {
            // Solo congelar si el objeto tiene la etiqueta "Player"
            if (rb != null && rb.gameObject.CompareTag("Player")) // rb puede ser null si el objeto fue destruido (poco probable aquí)
            {
                rb.isKinematic = true; // Congelar el Rigidbody
                rb.velocity = Vector3.zero; // Detener cualquier movimiento residual
                rb.angularVelocity = Vector3.zero; // Detener cualquier rotación residual
                rb.useGravity = false; // Desactivar la gravedad para que floten en el aire
            }
        }

        // Notificamos al controlador principal que el juego ha terminado.
        if (gameController != null)
        {
            gameController.LoseGame();
        }
    }

    // ... (El resto de tus funciones como OnDrawGizmosSelected no necesitan cambios) ...
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDePatrulla);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeEscucha);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radioDeVision);
        Vector3 anguloVisionA = DireccionDesdeAngulo(-anguloDeVision / 2, false);
        Vector3 anguloVisionB = DireccionDesdeAngulo(anguloDeVision / 2, false);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + anguloVisionA * radioDeVision);
        Gizmos.DrawLine(transform.position, transform.position + anguloVisionB * radioDeVision);
        if (puedeVerAlJugador)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
        else if (currentState == State.Buscando)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, ultimaPosicionConocidaDelJugador);
        }
    }
    private Vector3 DireccionDesdeAngulo(float anguloEnGrados, bool anguloEsGlobal)
    {
        if (!anguloEsGlobal)
        {
            anguloEnGrados += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(anguloEnGrados * Mathf.Deg2Rad), 0, Mathf.Cos(anguloEnGrados * Mathf.Deg2Rad));
    }
}
