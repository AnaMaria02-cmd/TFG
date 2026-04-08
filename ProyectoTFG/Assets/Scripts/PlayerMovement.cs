using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("Velocidad a la que gira el personaje. Menor valor = giro más suave/gradual.")]
    public float rotationSpeed = 10f;
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;

    private Rigidbody rb;
    private float moveX;
    private float moveZ;
    private bool isGrounded;
    private bool jumpIntent;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
