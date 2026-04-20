using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("Velocidad a la que gira el personaje. Menor valor = giro más suave/gradual.")]
    public float rotationSpeed = 10f;
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip footstepSound;
    public float stepInterval = 0.4f;

    private float stepTimer;
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private Rigidbody rb;
    private float moveX;
    private float moveZ;
    private bool isGrounded;
    private bool jumpIntent;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Configurar Música de fondo
        if (backgroundMusic != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.4f; 
            musicSource.playOnAwake = true;
            musicSource.Play();
        }

        // Configurar SFX (pasos)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 0.6f;
    }

    void Update()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        // Raycast desde un poco más abajo
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(
            rayStart,
            Vector3.down,
            groundCheckDistance
        );
        
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpIntent = true;
        }

        // Lógica de sonido de pasos
        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        if (movement.magnitude > 0.1f && isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (footstepSound != null && sfxSource != null)
                {
                    // Cambiar ligeramente el pitch (tono) en cada paso lo hace más realista
                    sfxSource.pitch = Random.Range(0.9f, 1.1f);
                    sfxSource.PlayOneShot(footstepSound);
                }
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // Reset para que suene de inmediato al arrancar
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        float currentVelocityY = rb.linearVelocity.y;
        if (jumpIntent)
        {
            currentVelocityY = jumpForce;
            jumpIntent = false;
        }

        rb.linearVelocity = new Vector3(
            movement.x * speed,
            currentVelocityY,
            movement.z * speed
        );

    }
}
