using UnityEngine;

public class NewCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Object to look at (e.g., Player)
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 10f;

    [Header("Rotation Settings")]
    public float sensitivityX = 4f;
    public float sensitivityY = 2f;
    public float minY = 10f;
    public float maxY = 80f;

    [Header("Zoom Settings")]
    public float storedZoom = 10f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    public float zoomSpeed = 5f;

    private float currentX = 0f;
    private float currentY = 45f;
    private float currentZoom = 10f;

    private void Start()
    {
        // Initialize rotation based on current offset direction if target exists
        if (target != null)
        {
            Vector3 diff = transform.position - target.position;
            currentZoom = diff.magnitude;
            storedZoom = currentZoom;
            // Optionally set initial angles here if needed, but defaults are usually fine
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraTransform();
    }

    private void HandleInput()
    {
        // Rotate with Right Mouse Button ONLY if NOT dragging a piece
        if (Input.GetMouseButton(1) && !DropAndDrag.IsDraggingAnyPiece)
        {
            currentX += Input.GetAxis("Mouse X") * sensitivityX;
            currentY -= Input.GetAxis("Mouse Y") * sensitivityY;

            currentY = Mathf.Clamp(currentY, minY, maxY);
        }

        // Zoom with Scroll Wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }

    private void UpdateCameraTransform()
    {
        // Calculate rotation and position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -currentZoom);
        
        Vector3 desiredPosition = target.position + rotation * direction;

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f); // Look slightly above pivot
    }
}
