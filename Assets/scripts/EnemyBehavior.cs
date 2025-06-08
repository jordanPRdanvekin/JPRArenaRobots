using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Referencias Principales")]
    public NavMeshAgent agent;
    public Transform player;
    public GameObject gameOverCanvas; // Arrastra aquí el Canvas de Game Over desde la jerarquía

    // Máquina de Estados interna
    private enum State { Patrullando, Descansando, Persiguiendo, Buscando }
    private State currentState;

    [Header("Configuración de Patrulla")]
    public float radioDePatrulla = 20f;
    public float tiempoDeMovimiento = 10f; // Segundos que se mueve antes de descansar
    public float tiempoDeDescanso = 3f;  // Segundos que "respira"

    private Vector3 puntoDeDestino;
    private bool destinoFijado;
    private float temporizadorDeEstado;

    // --- NUEVAS VARIABLES PARA PATRULLAJE CÍCLICO ---
    [Header("Configuración de Retorno al Origen")]
    [Tooltip("Cada cuántos segundos el enemigo intentará volver a su punto de partida si no está persiguiendo al jugador.")]
    public float tiempoParaRegresar = 50f;

    private Vector3 posicionOriginal;
    private float temporizadorDeRegreso;
    // --- FIN DE NUEVAS VARIABLES ---

    [Header("Configuración de Detección")]
    public float radioDeEscucha = 3f; // Radio para detectar al jugador muy cerca, sin necesidad de verlo.
    public float radioDeVision = 15f;
    [Range(0, 360)]
    public float anguloDeVision = 90f;
    public LayerMask capaDelJugador; // Se usa para que la física solo detecte al jugador por su CAPA (Layer).
    public LayerMask capaDeObstaculos; // Se usa para que el Raycast de visión choque con paredes.

    private bool puedeVerAlJugador;
    private Vector3 ultimaPosicionConocidaDelJugador;

    [Header("Configuración de Persecución")]
    public float tiempoDeBusqueda = 20f; // Segundos que busca al jugador tras perderlo de vista

    void Start()
    {
        // Obtener componentes automáticamente si no se asignan en el inspector
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = 0f;

        // Encontrar al jugador por su Tag si no se asigna
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        // --- INICIALIZACIÓN DE NUEVAS VARIABLES ---
        posicionOriginal = transform.position; // Guardamos la posición inicial.
        temporizadorDeRegreso = tiempoParaRegresar; // Iniciamos el temporizador de regreso.
        // --- FIN DE INICIALIZACIÓN ---

        currentState = State.Patrullando;
        temporizadorDeEstado = tiempoDeMovimiento;
    }

    void Update()
    {
        ComprobarCampoDeVision();
        ManejarEstados();
    }

    /// <summary>
    /// Gestiona la máquina de estados del enemigo, decidiendo si patrulla, persigue, etc.
    /// Incluye la lógica para regresar a su posición original periódicamente.
    /// </summary>
    private void ManejarEstados()
    {
        // Lógica de retorno cíclico: si no está persiguiendo, avanza el temporizador para volver a la base.
        if (currentState != State.Persiguiendo && currentState != State.Buscando)
        {
            temporizadorDeRegreso -= Time.deltaTime;
            if (temporizadorDeRegreso <= 0f)
            {
                // Vuelve a la base y reinicia los temporizadores.
                currentState = State.Patrullando;
                agent.SetDestination(posicionOriginal);
                destinoFijado = true;
                temporizadorDeRegreso = tiempoParaRegresar;
                temporizadorDeEstado = tiempoDeMovimiento;
                return;
            }
        }

        // La detección del jugador siempre tiene la máxima prioridad.
        if (puedeVerAlJugador)
        {
            if (currentState != State.Persiguiendo)
            {
                currentState = State.Persiguiendo;
                temporizadorDeEstado = tiempoDeBusqueda;
                // Si ve al jugador, el temporizador de regreso se reinicia.
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
            case State.Patrullando:
                Patrullar();
                break;
            case State.Descansando:
                Descansar();
                break;
            case State.Persiguiendo:
                Perseguir();
                break;
            case State.Buscando:
                Buscar();
                break;
        }
    }

    /// <summary>
    /// El enemigo se mueve a un punto aleatorio o espera a llegar a su destino.
    /// Si se acaba el tiempo de movimiento, pasa a descansar.
    /// </summary>
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

    /// <summary>
    /// El enemigo se detiene por un breve período antes de volver a patrullar.
    /// </summary>
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

    /// <summary>
    /// El enemigo persigue activamente la posición actual del jugador.
    /// </summary>
    private void Perseguir()
    {
        agent.SetDestination(player.position);
        ultimaPosicionConocidaDelJugador = player.position;
    }

    /// <summary>
    /// Tras perder de vista al jugador, el enemigo se dirige a la última posición conocida.
    /// </summary>
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

    /// <summary>
    /// Calcula un nuevo punto aleatorio dentro del radio de patrulla y lo asigna como destino.
    /// </summary>
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

    /// <summary>
    /// Comprueba si el jugador está dentro del campo de visión o del radio de escucha.
    /// </summary>
    private void ComprobarCampoDeVision()
    {
        // Primero comprueba el radio de escucha cercano
        Collider[] collidersEnEscucha = Physics.OverlapSphere(transform.position, radioDeEscucha, capaDelJugador);
        if (collidersEnEscucha.Length > 0)
        {
            puedeVerAlJugador = true;
            return;
        }

        // Si no, comprueba el cono de visión
        Collider[] collidersEnRango = Physics.OverlapSphere(transform.position, radioDeVision, capaDelJugador);
        if (collidersEnRango.Length > 0)
        {
            Transform objetivo = collidersEnRango[0].transform;
            Vector3 direccionHaciaObjetivo = (objetivo.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, direccionHaciaObjetivo) < anguloDeVision / 2)
            {
                float distanciaHaciaObjetivo = Vector3.Distance(transform.position, objetivo.position);
                // Lanza un rayo para asegurarse de que no hay obstáculos en medio
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
    /// Se activa cuando el enemigo toca al jugador. Inicia la secuencia de Game Over.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡El enemigo ha tocado al jugador! Iniciando Game Over.");

            if (gameOverCanvas != null)
            {
                gameOverCanvas.SetActive(true);
            }
            else
            {
                Debug.LogError("¡El Canvas de Game Over no está asignado en el Inspector!");
            }

            Time.timeScale = 0f; // Pausa el juego
            this.enabled = false;
            agent.enabled = false;
        }
    }

    /// <summary>
    /// Dibuja los Gizmos en el editor de Unity para visualizar los radios de detección.
    /// </summary>
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

    /// <summary>
    /// Función de utilidad para calcular una dirección a partir de un ángulo.
    /// </summary>
    private Vector3 DireccionDesdeAngulo(float anguloEnGrados, bool anguloEsGlobal)
    {
        if (!anguloEsGlobal)
        {
            anguloEnGrados += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(anguloEnGrados * Mathf.Deg2Rad), 0, Mathf.Cos(anguloEnGrados * Mathf.Deg2Rad));
    }
}
