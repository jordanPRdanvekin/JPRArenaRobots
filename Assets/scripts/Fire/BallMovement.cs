using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el movimiento de un proyectil que busca enemigos, se atrae con otros proyectiles y se fusiona.
/// </summary>
public class BallMovement : MonoBehaviour
{
    // --- ESTADOS DEL PROYECTIL ---
    public enum ProjectileState { Charging, Firing }
    public ProjectileState state = ProjectileState.Charging;

    // --- Variables de Movimiento y Homing ---
    [Header("Configuración General")]
    [SerializeField] float velocity = 2;
    [Tooltip("El tamaño máximo que puede alcanzar el proyectil al cargarse.")]
    [SerializeField] private float maxScale = 3.0f;
    public Vector3 dire;

    [Header("Homing (Búsqueda de Enemigo)")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask projectileLayer;
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private float searchInterval = 0.25f;

    [Header("Fusión y Órbita")]
    [SerializeField] private float mergePowerUp = 1.25f;
    [SerializeField] private float friendAttraction = 0.75f;

    [Header("Ataque en Cadena (Rebote)")]
    [Tooltip("Factor por el que se encoge el proyectil al rebotar.")]
    [SerializeField] private float shrinkFactorOnBounce = 0.75f;
    private int bounceCharges = 0;

    // --- Variables Privadas ---
    private Transform currentTarget;
    private Collider targetCollider;
    private float searchTimer;
    private bool isDying = false;

    private void Start()
    {
        if (projectileLayer.value == 0)
        {
            Debug.LogWarning("ADVERTENCIA: La 'Projectile Layer' no está asignada en el Inspector.", this.gameObject);
        }

        Destroy(gameObject, 20);

        // El proyectil aparece muy pequeño.
        transform.localScale = Vector3.one * 0.1f;
    }

    private void Update()
    {
        // Mientras está en estado "Charging", no hace nada por sí mismo. El CombaPlayer lo controla.
        if (state == ProjectileState.Charging || isDying) return;

        HandleTargeting();
        MovementBall();
    }

    /// <summary>
    /// Establece cuántas veces puede rebotar este proyectil. Es llamado por el jugador.
    /// </summary>
    public void SetBounces(int bounces)
    {
        bounceCharges = bounces;
    }

    /// <summary>
    /// Aumenta el tamaño del proyectil mientras se carga, respetando un límite máximo.
    /// </summary>
    /// <param name="growthAmount">La cantidad a crecer en este frame.</param>
    public void Grow(float growthAmount)
    {
        if (state == ProjectileState.Charging && transform.localScale.x < maxScale)
        {
            Vector3 newScale = transform.localScale + Vector3.one * growthAmount;
            // Aseguramos que no sobrepase el tamaño máximo
            if (newScale.x > maxScale)
            {
                newScale = Vector3.one * maxScale;
            }
            transform.localScale = newScale;
        }
    }

    /// <summary>
    /// Libera el proyectil, permitiéndole moverse.
    /// </summary>
    public void Release()
    {
        state = ProjectileState.Firing;

        // Al ser liberado, establece su dirección de vuelo inicial basado en su rotación actual.
        dire = transform.forward;

        LeanTween.scale(gameObject, transform.localScale * 1.3f, 0.2f).setEasePunch();
    }

    private void HandleTargeting()
    {
        if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
        {
            currentTarget = null;
            targetCollider = null;
        }

        if (currentTarget == null)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                FindClosestEnemy();
                searchTimer = searchInterval;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDying || state == ProjectileState.Charging) return;

        if (other.TryGetComponent<BallMovement>(out BallMovement otherBall) && otherBall.state == ProjectileState.Firing)
        {
            if (other.gameObject.GetInstanceID() > gameObject.GetInstanceID())
            {
                transform.localScale *= mergePowerUp;
                velocity *= mergePowerUp;
                otherBall.isDying = true;
                Destroy(other.gameObject);
            }
            return;
        }

        if (other.CompareTag("enemigo"))
        {
            if (bounceCharges > 0)
            {
                bounceCharges--;
                Destroy(other.gameObject);

                LeanTween.scale(gameObject, transform.localScale * shrinkFactorOnBounce, 0.2f);
                currentTarget = null;
            }
            else
            {
                isDying = true;
                Destroy(other.gameObject);
                StartCoroutine(DestroySelfAfterDelay(0.05f));
            }
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    void MovementBall()
    {
        Vector3 finalDirection;

        Vector3 friendInfluence = Vector3.zero;
        Transform closestFriend = FindClosestFriend();
        if (closestFriend != null)
        {
            Vector3 directionToFriend = (closestFriend.position - transform.position).normalized;
            friendInfluence = directionToFriend * friendAttraction;
        }

        if (currentTarget != null && targetCollider != null)
        {
            Vector3 directionToTarget = (targetCollider.bounds.center - transform.position).normalized;
            finalDirection = (directionToTarget + friendInfluence).normalized;
        }
        else
        {
            Vector3 forwardDirection = dire;
            finalDirection = (forwardDirection + friendInfluence).normalized;
        }

        dire = Vector3.Slerp(dire, finalDirection, homingStrength * Time.deltaTime);
        transform.position += dire * velocity * Time.deltaTime;

        if (dire != Vector3.zero)
        {
            transform.forward = dire;
        }
    }

    void FindClosestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Collider bestTargetCollider = null;

        foreach (var enemyCollider in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                bestTargetCollider = enemyCollider;
            }
        }

        if (bestTargetCollider != null)
        {
            currentTarget = bestTargetCollider.transform;
            targetCollider = bestTargetCollider;
        }
    }

    Transform FindClosestFriend()
    {
        Collider[] friends = Physics.OverlapSphere(transform.position, detectionRadius, projectileLayer);

        float closestDistance = Mathf.Infinity;
        Transform bestFriend = null;

        foreach (var friendCollider in friends)
        {
            if (friendCollider.gameObject == this.gameObject || friendCollider.GetComponent<BallMovement>().state == ProjectileState.Charging) continue;

            float distanceToFriend = Vector3.Distance(transform.position, friendCollider.transform.position);
            if (distanceToFriend < closestDistance)
            {
                closestDistance = distanceToFriend;
                bestFriend = friendCollider.transform;
            }
        }
        return bestFriend;
    }

    IEnumerator DestroySelfAfterDelay(float delay)
    {
        if (GetComponent<Renderer>() != null) GetComponent<Renderer>().enabled = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
