using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Camera playerCamera;

    [Header("Movimiento")]
    [Tooltip("Velocidad de movimiento regulable en el editor.")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Cámara y FOV")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minFOV = 50f;
    [SerializeField] private float maxFOV = 120f;
    [SerializeField] private float fovChangeSpeed = 10f;
    private float targetFOV;
    private float cameraPitch = 0f;

    [Header("Vida")]
    [Tooltip("Vida actual del personaje (Valor inicial: 100).")]
    [SerializeField] private float health = 100f;
    private bool isDead = false;

    [Header("Estamina y Salto")]
    [Tooltip("Estamina actual (Valor inicial: 10, Máximo: 10).")]
    [SerializeField] private float stamina = 10f;
    private const float maxStamina = 10f;
    [Tooltip("Fuerza de salto regulable en el editor.")]
    [SerializeField] private float jumpForce = 5f;
    [Tooltip("Estamina que consume cada salto (Valor: 5).")]
    [SerializeField] private float jumpStaminaCost = 5f;
    private bool isGrounded = false;

    [Header("Arma (Raycast / Pistola no automática)")]
    [SerializeField] private float weaponRange = 20f;
    [SerializeField] private float weaponDamage = 25f;
    [SerializeField] private float fireRate = 1.5f;
    private float nextFireTime = 0f;
    [SerializeField] private int bullets = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            targetFOV = playerCamera.fieldOfView;
        }

        // Bloquear cursor al centro para la vista de FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Si la vida llega a 0, el personaje no puede moverse ni atacar
        if (isDead) return;

        HandleLook();
        HandleFOV();
        HandleStaminaRegen();
        HandleJump();
        HandleShooting();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        HandleMovement();
    }

    // --- MOVIMIENTO (WASD / Estándar FPS) ---
    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 moveDirection = transform.forward * moveZ + transform.right * moveX;
        moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Mantener la gravedad actual

        rb.linearVelocity = targetVelocity;
    }

    // --- MÉTODO PARA RECOGER MUNICIÓN ---
    public void AddAmmo(int amount)
    {
        bullets += amount;
        Debug.Log("Balas actuales: " + bullets);
    }

    // --- CÁMARA EN PRIMERA PERSONA Y RATÓN ---
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotar jugador horizontalmente
        transform.Rotate(Vector3.up * mouseX);

        // Rotar cámara verticalmente con límites
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    // --- MODIFICACIÓN DE FOV (T e Y) ---
    void HandleFOV()
    {
        if (playerCamera == null) return;

        // T baja FOV, Y sube FOV (Mínimo 50, Máximo 120)
        if (Input.GetKey(KeyCode.T))
        {
            targetFOV -= fovChangeSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Y))
        {
            targetFOV += fovChangeSpeed * Time.deltaTime;
        }

        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 10f);
    }

    // --- ESTAMINA Y REGENERACIÓN ---
    void HandleStaminaRegen()
    {
        // La estamina sube automáticamente 2 puntos por segundo, sin pasar de 10 ni bajar de 0
        if (stamina < maxStamina)
        {
            stamina += 2f * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
    }

    // --- DETECCIÓN DE PISO (Colisiones) ---
    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // --- SALTO ---
    void HandleJump()
    {
        // Salta si está en el piso y gasta 5 de estamina
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (stamina >= jumpStaminaCost)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
                stamina -= jumpStaminaCost;
                stamina = Mathf.Clamp(stamina, 0f, maxStamina);
            }
        }
    }

    // --- ATAQUE (Raycast al centro de la pantalla / Pistola no automática) ---
    void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && bullets > 0)
        {
            nextFireTime = Time.time + fireRate;
            bullets--;

            FireRaycast();
        }
    }

    void FireRaycast()
    {
        if (playerCamera == null) return;

        // Disparo desde el centro exacto de la pantalla (0.5, 0.5)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, weaponRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // Aquí se conectará con el script de vida del enemigo cuando lo crees
                Debug.Log("¡Enemigo impactado! Daño: " + weaponDamage);
            }
        }
    }

    // --- SISTEMA DE VIDA ---
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        health = Mathf.Max(health, 0f);

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("El personaje ha muerto. No puede moverse ni atacar.");
    }
}