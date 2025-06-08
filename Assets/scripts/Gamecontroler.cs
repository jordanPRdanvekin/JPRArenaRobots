using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para las Corutinas

public class GameController : MonoBehaviour
{
    [Header("Referencias de Pantallas")]
    [Tooltip("Arrastra aquí el objeto Canvas de la pantalla de victoria.")]
    public VictoryScreenController victoryScreenController;
    [Tooltip("Arrastra aquí el objeto Canvas de la pantalla de Game Over.")]
    public GameOverController gameOverController;

    [Header("Configuración de Victoria")]
    [Tooltip("La etiqueta que identifica a todos tus enemigos.")]
    public string enemyTag = "enemigo";

    [Header("Configuración de Pausa")]
    [Tooltip("Segundos a esperar antes de congelar el juego, para dar tiempo a la animación de UI.")]
    public float delayBeforeFreeze = 1.5f;

    private float startTime;
    private bool isGameRunning = false;

    void Start()
    {
        if (victoryScreenController == null || gameOverController == null)
        {
            Debug.LogError("¡ERROR CRÍTICO! Asigna las pantallas de Victoria y Game Over en el GameController.");
            this.enabled = false;
            return;
        }

        // Aseguramos que las pantallas empiecen desactivadas y el tiempo sea normal
        Time.timeScale = 1f;
        victoryScreenController.gameObject.SetActive(false);
        gameOverController.gameObject.SetActive(false);

        InitializeGame();
    }

    void InitializeGame()
    {
        startTime = Time.time;
        isGameRunning = true;
        Debug.Log("Partida iniciada.");
    }

    void Update()
    {
        if (!isGameRunning) return;

        // Comprobamos la condición de victoria
        if (GameObject.FindGameObjectWithTag(enemyTag) == null)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        if (!isGameRunning) return;
        isGameRunning = false;
        float elapsedTime = Time.time - startTime;

        Debug.Log("¡VICTORIA! Iniciando secuencia...");

        // 1. Llamamos a la animación de la interfaz de victoria
        victoryScreenController.TriggerVictorySequence(elapsedTime);

        // 2. Congelamos el juego DESPUÉS de una pequeña pausa
        StartCoroutine(FreezeGameAfterDelay());
    }

    public void LoseGame()
    {
        if (!isGameRunning) return;
        isGameRunning = false;
        float elapsedTime = Time.time - startTime;

        Debug.Log("¡DERROTA! Iniciando secuencia...");

        // Detenemos a todos los enemigos para que no sigan moviéndose
        DisableAllEnemies();

        // 1. Llamamos a la animación de la interfaz de Game Over
        gameOverController.TriggerGameOverSequence(elapsedTime);

        // 2. Congelamos el juego DESPUÉS de una pequeña pausa
        StartCoroutine(FreezeGameAfterDelay());
    }

    /// <summary>
    /// Desactiva la IA de todos los enemigos en la escena.
    /// </summary>
    private void DisableAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            // Desactivamos el agente de navegación y el script de comportamiento
            if (enemy.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            if (enemy.TryGetComponent<EnemyBehavior>(out var behavior))
            {
                behavior.enabled = false;
            }
        }
    }

    /// <summary>
    /// Corutina que espera un momento antes de congelar el tiempo y liberar el cursor.
    /// </summary>
    private IEnumerator FreezeGameAfterDelay()
    {
        // Esperamos a que la animación de la UI haya empezado a reproducirse
        yield return new WaitForSecondsRealtime(delayBeforeFreeze); // Usamos Realtime para que no le afecte el Time.timeScale

        // Ahora, congelamos el tiempo y gestionamos el cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
