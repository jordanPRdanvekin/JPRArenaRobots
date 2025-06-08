using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombaPlayer : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("Tiempo en segundos entre cada nuevo par de proyectiles mientras se carga.")]
    [SerializeField] private float chargeSpawnRate = 0.3f;
    [Tooltip("Velocidad a la que crecen los proyectiles. Ajusta este valor para definir cuánto tardan en llegar al tamaño máximo.")]
    [SerializeField] private float growthRate = 0.29f;

    [Header("Configuración de Cargas (Rebotes)")]
    [SerializeField] private int maxBounceCharges = 5;
    [Tooltip("Tiempo en segundos para ganar una carga de rebote.")]
    [SerializeField] private float bounceChargeGainInterval = 10f;
    private int currentBounceCharges;
    private float bounceChargeTimer;

    // --- Variables de Carga ---
    private List<BallMovement> chargedProjectiles = new List<BallMovement>();
    private bool isCharging = false;
    private float chargeSpawnTimer;

    private void Update()
    {
        HandleBounceCharges();
        HandleInput();
    }

    /// <summary>
    /// Gestiona la ganancia de cargas de rebote a lo largo del tiempo.
    /// </summary>
    private void HandleBounceCharges()
    {
        if (currentBounceCharges < maxBounceCharges)
        {
            bounceChargeTimer += Time.deltaTime;
            if (bounceChargeTimer >= bounceChargeGainInterval)
            {
                bounceChargeTimer = 0f;
                currentBounceCharges++;
                Debug.Log("Carga de rebote ganada! Total: " + currentBounceCharges);
            }
        }
    }

    /// <summary>
    /// Gestiona la entrada del jugador para cargar y disparar.
    /// </summary>
    private void HandleInput()
    {
        // Cuando el jugador PRESIONA el botón de disparo
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeSpawnTimer = chargeSpawnRate; // Para que instancie el primero al instante
        }

        // Mientras el jugador MANTIENE PRESIONADO el botón
        if (isCharging && Input.GetMouseButton(0))
        {
            // 1. Instancia nuevos proyectiles según la cadencia de carga
            chargeSpawnTimer += Time.deltaTime;
            if (chargeSpawnTimer >= chargeSpawnRate)
            {
                chargeSpawnTimer = 0f;
                InstanceAndHoldBall();
            }

            // 2. Hace que todos los proyectiles cargados sigan al jugador y crezcan
            for (int i = 0; i < chargedProjectiles.Count; i++)
            {
                BallMovement ball = chargedProjectiles[i];
                if (ball != null)
                {
                    // Determina a qué spawnPoint debe seguir, ciclando si hay más proyectiles que puntos
                    Transform targetSpawnPoint = spawnPoints[i % spawnPoints.Length];

                    // Actualiza posición y rotación para que siga al jugador
                    ball.transform.position = targetSpawnPoint.position;
                    ball.transform.rotation = targetSpawnPoint.rotation;

                    // Llama al método Grow para aumentar el tamaño
                    ball.Grow(growthRate * Time.deltaTime);
                }
            }
        }

        // Cuando el jugador SUELTA el botón de disparo
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            isCharging = false;
            ReleaseAllBalls();
        }
    }

    /// <summary>
    /// Instancia un proyectil por cada punto de spawn y lo añade a la lista de espera.
    /// </summary>
    void InstanceAndHoldBall()
    {
        if (spawnPoints.Length == 0) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject ballGO = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
            BallMovement ball = ballGO.GetComponent<BallMovement>();

            if (ball != null)
            {
                ball.SetBounces(currentBounceCharges);
                chargedProjectiles.Add(ball);
            }
        }
    }

    /// <summary>
    /// Libera todos los proyectiles cargados y consume las cargas de rebote.
    /// </summary>
    void ReleaseAllBalls()
    {
        foreach (BallMovement ball in chargedProjectiles)
        {
            if (ball != null)
            {
                ball.Release();
            }
        }

        currentBounceCharges = 0;
        bounceChargeTimer = 0f;

        chargedProjectiles.Clear();
    }
}
