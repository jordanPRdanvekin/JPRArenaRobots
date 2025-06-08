using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Agregado para NavMeshAgent, ya que se referencia en el c�digo

/// <summary>
/// Gestiona el movimiento de un proyectil que busca enemigos, se atrae con otros proyectiles y se fusiona.
/// </summary>
public class BallMovement : MonoBehaviour
{
    // --- ESTADOS DEL PROYECTIL ---
    public enum ProjectileState { Charging, Firing }
    public ProjectileState state = ProjectileState.Charging;

    // --- Variables de Movimiento y Homing ---
    [Header("Configuraci�n General")]
    [SerializeField] float velocity = 2;
    [Tooltip("El tama�o m�ximo que puede alcanzar el proyectil al cargarse.")]
    [SerializeField] private float maxScale = 3.0f;
    public Vector3 dire;

    [Header("Homing (B�squeda de Enemigo)")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask projectileLayer;
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private float searchInterval = 0.25f;

    [Header("Fusi�n y �rbita")]
    [SerializeField] private float mergePowerUp = 1.25f;
    [SerializeField] private float friendAttraction = 0.75f;

    [Header("Ataque en Cadena (Rebote)")]
    [Tooltip("Factor por el que se encoge el proyectil al rebotar.")]
    [SerializeField] private float shrinkFactorOnBounce = 0.75f;
    private int bounceCharges = 0;

    // --- NUEVAS VARIABLES PARA LA DESINTEGRACI�N DEL ENEMIGO ---
    [Header("Configuraci�n de Desintegraci�n del Enemigo")]
    [Tooltip("Fuerza con la que se dispersan las piezas del enemigo.")]
    public float enemyDisintegrateForce = 300f;
    [Tooltip("Fuerza de rotaci�n aleatoria para las piezas del enemigo.")]
    public float enemyDisintegrateRotationForce = 50f;
    [Tooltip("Duraci�n de la animaci�n de encogimiento de las piezas del enemigo.")]
    public float enemyDisintegrateShrinkDuration = 1.0f;
    [Tooltip("Determina si el enemigo puede ser impactado cuando ya est� desintegr�ndose.")]
    public bool allowImpactDuringDisintegration = false;
    [Tooltip("Prefab de la explosi�n de part�culas a instanciar cuando una pieza se destruye.")]
    public GameObject explosionParticlePrefab;
    [Tooltip("Tiempo en segundos que la explosi�n de part�culas permanecer� en la escena.")]
    public float explosionParticleLifetime = 2f;

    // --- NUEVAS VARIABLES PARA LA CONFIGURACI�N DE REBOTE ---
    [Header("Configuraci�n de Rebote")]
    [Tooltip("N�mero m�ximo de rebotes que la bala puede hacer en superficies que no son enemigos.")]
    public int maxBounces = 2;
    private int currentBounces = 0; // Contador de rebotes actuales


    // --- Variables Privadas ---
    private Transform currentTarget;
    private Collider targetCollider;
    private float searchTimer;
    private bool isDying = false; // Esta bandera es para el proyectil, no para el enemigo

    private void Start()
    {
        if (projectileLayer.value == 0)
        {
            Debug.LogWarning("ADVERTENCIA: La 'Projectile Layer' no est� asignada en el Inspector.", this.gameObject);
        }

        Destroy(gameObject, 20);

        // El proyectil aparece muy peque�o.
        transform.localScale = Vector3.one * 0.1f;
    }

    private void Update()
    {
        // Mientras est� en estado "Charging", no hace nada por s� mismo. El CombaPlayer lo controla.
        if (state == ProjectileState.Charging || isDying) return;

        HandleTargeting();
        MovementBall();
    }

    /// <summary>
    /// Establece cu�ntas veces puede rebotar este proyectil. Es llamado por el jugador.
    /// </summary>
    public void SetBounces(int bounces)
    {
        bounceCharges = bounces;
    }

    /// <summary>
    /// Aumenta el tama�o del proyectil mientras se carga, respetando un l�mite m�ximo.
    /// </summary>
    /// <param name="growthAmount">La cantidad a crecer en este frame.</param>
    public void Grow(float growthAmount)
    {
        if (state == ProjectileState.Charging && transform.localScale.x < maxScale)
        {
            Vector3 newScale = transform.localScale + Vector3.one * growthAmount;
            // Aseguramos que no sobrepase el tama�o m�ximo
            if (newScale.x > maxScale)
            {
                newScale = Vector3.one * maxScale;
            }
            transform.localScale = newScale;
        }
    }

    /// <summary>
    /// Libera el proyectil, permiti�ndole moverse.
    /// </summary>
    public void Release()
    {
        state = ProjectileState.Firing;

        // Al ser liberado, establece su direcci�n de vuelo inicial basado en su rotaci�n actual.
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

        // Colisi�n con otra bala (fusi�n)
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

        // Colisi�n con un enemigo
        if (other.CompareTag("enemigo"))
        {
            // Antes de hacer cualquier cosa, verifica si el enemigo ya est� en proceso de desintegraci�n
            // Necesitar�s un script en el enemigo que maneje su estado de desintegraci�n
            // Por ejemplo, si el enemigo tuviera un script llamado 'EnemyHealth' con una bandera 'isDisintegrating'
            // EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            // if (enemyHealth != null && enemyHealth.isDisintegrating && !allowImpactDuringDisintegration) return;

            // Inicia la corrutina de desintegraci�n para el enemigo golpeado
            StartCoroutine(DisintegrateEnemyParts(other.gameObject));

            if (bounceCharges > 0)
            {
                bounceCharges--;
                // Si hay rebotes, la bala contin�a despu�s de golpear
                LeanTween.scale(gameObject, transform.localScale * shrinkFactorOnBounce, 0.2f);
                currentTarget = null; // Reiniciar la b�squeda de objetivo
            }
            else
            {
                // Si no hay rebotes, la bala se destruye
                isDying = true;
                StartCoroutine(DestroySelfAfterDelay(0.05f));
            }
            return;
        }

        // Si colisiona con algo que no es un jugador, enemigo o otra bala (estos casos ya fueron manejados por los 'return' anteriores)
        // entonces es una superficie de rebote.
        if (!other.CompareTag("Player")) // Esta condici�n ya es suficiente porque las otras colisiones ya han hecho 'return'
        {
            if (currentBounces < maxBounces)
            {
                currentBounces++;
                dire = -dire; // Rebote simple: invertir la direcci�n
                // Empujar la bala un poco para evitar que se quede pegada al collider
                transform.position += dire.normalized * 0.2f; // Usar normalized para un empuje constante
            }
            else
            {
                // Si se exceden los rebotes permitidos, destruir la bala.
                isDying = true;
                StartCoroutine(DestroySelfAfterDelay(0.05f)); // Usar el mismo mecanismo de autodestrucci�n.
            }
            return; // Salir despu�s de manejar el rebote
        }
    }

    /// <summary>
    /// Coroutine para desintegrar las partes de un enemigo.
    /// </summary>
    /// <param name="enemyRoot">El GameObject ra�z del enemigo a desintegrar.</param>
    IEnumerator DisintegrateEnemyParts(GameObject enemyRoot)
    {
        if (enemyRoot == null) yield break; // Salir si el objeto ya no existe

        // Desactivar el collider principal y el renderer del enemigo ra�z
        // para que parezca que desaparece mientras sus partes se dispersan
        Collider mainCollider = enemyRoot.GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        Renderer mainRenderer = enemyRoot.GetComponent<Renderer>();
        if (mainRenderer != null) mainRenderer.enabled = false;

        // Desactivar cualquier script EnemyBehavior en el enemigo si existe
        EnemyBehavior enemyBehavior = enemyRoot.GetComponent<EnemyBehavior>();
        if (enemyBehavior != null)
        {
            enemyBehavior.enabled = false;
            // Si el enemigo tiene un NavMeshAgent, tambi�n desact�valo
            NavMeshAgent navAgent = enemyRoot.GetComponent<NavMeshAgent>();
            if (navAgent != null) navAgent.enabled = false;
        }


        // Obtener todos los Rigidbody en los hijos (y el propio GameObject si lo tiene)
        // Se asume que las "piezas" del enemigo son Rigidbodies hijos.
        Rigidbody[] enemyPartsRbs = enemyRoot.GetComponentsInChildren<Rigidbody>();

        List<GameObject> actualPartsToDisintegrate = new List<GameObject>();

        // Si el enemigo principal tiene Rigidbody y no tiene hijos con Rigidbody, lo consideramos la �nica "pieza"
        if (enemyPartsRbs.Length == 0 && enemyRoot.TryGetComponent<Rigidbody>(out Rigidbody rootRb))
        {
            actualPartsToDisintegrate.Add(rootRb.gameObject);
        }
        else
        {
            // A�adir todas las partes hijas con Rigidbody a la lista
            foreach (Rigidbody rb in enemyPartsRbs)
            {
                actualPartsToDisintegrate.Add(rb.gameObject);
            }
        }

        foreach (GameObject part in actualPartsToDisintegrate)
        {
            if (part == null) continue;

            Rigidbody rb = part.GetComponent<Rigidbody>();
            if (rb == null) continue; // Si la parte no tiene Rigidbody, la ignoramos para la f�sica

            rb.isKinematic = false; // Habilitar la f�sica
            rb.useGravity = true;   // Activar la gravedad

            // Calcular una direcci�n aleatoria de dispersi�n con un componente hacia arriba
            Vector3 randomDirection = Random.onUnitSphere;
            randomDirection.y = Mathf.Abs(randomDirection.y) + 0.2f; // Un peque�o empuje hacia arriba
            randomDirection.Normalize();

            // Aplicar fuerza y rotaci�n
            rb.AddForce(randomDirection * enemyDisintegrateForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * enemyDisintegrateRotationForce, ForceMode.Impulse);

            // �IMPORTANTE! Desvincular la pieza de su padre
            rb.transform.parent = null;

            // Animaci�n de encogimiento y desvanecimiento con LeanTween
            LeanTween.scale(part, Vector3.zero, enemyDisintegrateShrinkDuration)
                .setEase(LeanTweenType.easeInQuad) // Puedes ajustar el tipo de easing
                .setOnComplete(() => {
                    // Instanciar la explosi�n con rotaci�n aleatoria y autodestrucci�n
                    if (explosionParticlePrefab != null)
                    {
                        GameObject instantiatedExplosion = Instantiate(explosionParticlePrefab, part.transform.position, Random.rotation); // �ROTACI�N ALEATORIA!
                        // Adicionalmente, encoge la explosi�n para el efecto de "evaporaci�n"
                        LeanTween.scale(instantiatedExplosion, Vector3.zero, explosionParticleLifetime)
                            .setEase(LeanTweenType.easeOutQuad); // Ajusta el tipo de easing si lo deseas
                        Destroy(instantiatedExplosion, explosionParticleLifetime); // �AUTODESTRUCCI�N!
                    }
                    if (part != null) Destroy(part); // Destruir la pieza al finalizar el encogimiento
                });

            // Animaci�n de desvanecimiento (opcional, requiere un Renderer y material compatible con alpha)
            Renderer partRenderer = part.GetComponent<Renderer>();
            if (partRenderer != null && partRenderer.material.HasProperty("_Color")) // Verificar si el material tiene un color que se pueda modificar
            {
                Color originalColor = partRenderer.material.color;
                LeanTween.value(part, originalColor.a, 0f, enemyDisintegrateShrinkDuration)
                    .setOnUpdate((float value) => {
                        Color currentColor = partRenderer.material.color;
                        currentColor.a = value;
                        partRenderer.material.color = currentColor;
                    });
            }
        }

        // Esperar la duraci�n del encogimiento para permitir que las animaciones se completen
        yield return new WaitForSeconds(enemyDisintegrateShrinkDuration + 0.1f);

        // Finalmente, destruir el GameObject ra�z del enemigo si a�n existe
        if (enemyRoot != null)
        {
            Destroy(enemyRoot);
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
        transform.forward = dire; // Aseg�rate de que la bala mira hacia donde se mueve
        transform.position += dire * velocity * Time.deltaTime;

        // Nota: la l�nea anterior `if (dire != Vector3.zero) { transform.forward = dire; }` es redundante
        // ya que la direcci�n se actualiza en cada frame. Solo se necesita `transform.forward = dire;`
    }

    void FindClosestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Collider bestTargetCollider = null;

        foreach (var enemyCollider in enemies)
        {
            // Solo buscar enemigos que no est�n ya en proceso de desintegraci�n si esa opci�n est� activada
            // Comprobaci�n de que el Collider est� habilitado (ya que se deshabilitar� al iniciar la desintegraci�n)
            if (enemyCollider.enabled)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemyCollider.transform.position);
                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    bestTargetCollider = enemyCollider;
                }
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
