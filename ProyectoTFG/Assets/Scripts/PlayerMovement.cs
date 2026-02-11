using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;

    private Rigidbody rb;
    private float moveX;
    private float moveZ;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        // Raycast desde un poco más abajo
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance
        );

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        /*
        // Rotación
        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.fixedDeltaTime);
        }*/

        rb.linearVelocity = new Vector3(
            movement.x * speed,
            rb.linearVelocity.y,
            movement.z * speed
        );

    }
}
