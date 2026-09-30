using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    private Rigidbody rb;
    private Transform playerTransform;

    [Header("Movimiento y Persecución")]
    [Tooltip("Velocidad de movimiento del enemigo regulable en el editor.")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Distancia máxima a la que el enemigo comienza a perseguir al jugador (Valor: 5).")]
    [SerializeField] private float detectionRange = 5f;

    [Header("Vida del Enemigo")]
    [Tooltip("Vida actual del enemigo (Valor: 100).")]
    [SerializeField] private float enemyHealth = 100f;
    private bool isDead = false;

    [Header("Ataque del Enemigo (Raycast / Pistola no automática)")]
    [Tooltip("Rango máximo del ataque del enemigo (Valor: 5).")]
    [SerializeField] private float attackRange = 5f;
    [Tooltip("Daño que inflige el ataque del enemigo (Valor: 20).")]
    [SerializeField] private float attackDamage = 20f;
    [Tooltip("Cadencia de ataque en segundos (Valor: 2).")]
    [SerializeField] private float fireRate = 2f;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Buscar automáticamente al jugador en la escena mediante su Tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        // Si el enemigo muere, no se mueve ni ataca (pero sigue presente)
        if (isDead) return;

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Persecución si está dentro de la distancia configurada (5m)
            if (distanceToPlayer <= detectionRange)
            {
                MoveTowardsPlayer();

                // Intentar atacar si está a rango
                if (distanceToPlayer <= attackRange)
                {
                    TryAttackPlayer();
                }
            }
            else
            {
                // Si el jugador sale del rango, detiene su movimiento horizontal
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // Calcular dirección hacia el jugador
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Mantener movimiento en el plano horizontal

        // Rotar hacia el jugador
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Mover usando el Rigidbody
        Vector3 targetVelocity = direction * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Mantener gravedad
        rb.linearVelocity = targetVelocity;
    }

    void TryAttackPlayer()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            ShootAtPlayer();
        }
    }

    void ShootAtPlayer()
    {
        // Simulamos el Raycast de ataque del enemigo hacia el jugador
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Altura del pecho/ojos del enemigo
        Vector3 rayDirection = (playerTransform.position - rayOrigin).normalized;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Intentamos aplicar daño al script del jugador (PlayerController)
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                    Debug.Log("¡El enemigo ha disparado al jugador! Daño: " + attackDamage);
                }
            }
        }
    }

    // Método para recibir daño del jugador
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        enemyHealth -= amount;
        enemyHealth = Mathf.Max(enemyHealth, 0f);
        Debug.Log("Vida del enemigo restante: " + enemyHealth);

        if (enemyHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        // Detener por completo su movimiento físico pero dejarlo visible en pantalla
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        Debug.Log("El enemigo ha muerto: ya no se mueve ni ataca, pero sigue presente.");
    }
}