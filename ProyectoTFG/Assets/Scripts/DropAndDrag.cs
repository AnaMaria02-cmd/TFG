using System.Collections.Generic;
using UnityEngine;

public class DropAndDrag : MonoBehaviour
{
    Vector3 mouseOffset;
    Vector3 worldPosition;

    public float snapDistance = 1f;
    public List<Transform> nodes = new List<Transform>();
    public Transform playerTransform;

    public static bool IsDraggingAnyPiece = false; // New static flag

    bool isAttached = false;
    bool isSelected = false;
    Transform attachedNode;

    private Plane dragPlane;
    private Camera cam;
    private Rigidbody rb;

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        // Si empieza suelta, activar física normal
        if (rb != null && !isAttached)
            rb.isKinematic = false;

        // Las piezas no deben bloquear ni empujar al jugador
      
    }

   
    private void Update()
    {
        // Rotación solo si está seleccionada
        if (isSelected && Input.GetMouseButtonDown(1))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.Rotate(90f, 0f, 0f, Space.World);
            }
            else
            {
                transform.Rotate(0f, 90f, 0f, Space.World);
            }
        }
    }

    private void OnMouseDown()
    {
        isSelected = true;
        IsDraggingAnyPiece = true; // Set flag

        // Al coger la pieza: kinematic para que no choque mientras se arrastra
        if (rb != null) rb.isKinematic = true;

        // Plano que bloquea eje Z
        dragPlane = new Plane(Vector3.forward, transform.position);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            mouseOffset = transform.position - ray.GetPoint(distance);
        }

        // Si estaba ensamblada → desensamblar
        if (isAttached)
        {
            Socket socket = attachedNode.GetComponent<Socket>();
            if (socket != null)
                socket.isOccupied = false;

            // Quitar de la jerarquía y devolver física
            transform.SetParent(null, true);
            if (rb != null) rb.isKinematic = false;

            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                // Comprobar con childCount > 0 (más fiable que isOccupied,
                // que puede estar desincronizado). Si tiene hijos, significa
                // que hay una pieza conectada → no desactivar para no apagar
                // esa pieza (ej. pieza B) junto con el socket.
                if (s.transform.childCount > 0)
                    continue;

                s.gameObject.SetActive(false);
            }

            attachedNode = null;
            isAttached = false;
        }
    }

    private void OnMouseDrag()
    {
        if (isAttached) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            transform.position = point + mouseOffset;
            worldPosition = transform.position;
        }
    }

    private void OnMouseUp()
    {
        isSelected = false;
        IsDraggingAnyPiece = false; // Reset flag

        // Solo devolver física si NO encajó en socket (se gestiona abajo)

        if (isAttached) return;

        Socket closestSocket = null;

        // 🔹 USAR SOCKET MANAGER (forma correcta)
        if (SocketManager.Instance != null)
        {
            closestSocket = SocketManager.Instance.GetClosestSocket(worldPosition, snapDistance);
        }
        else
        {
            // 🔹 Fallback por si no existe el manager
            float smallestDistance = snapDistance;

            foreach (Socket socket in FindObjectsOfType<Socket>())
            {
                if (socket == null || socket.isOccupied || !socket.gameObject.activeInHierarchy)
                    continue;

                float d = Vector3.Distance(socket.transform.position, worldPosition);

                if (d < smallestDistance)
                {
                    smallestDistance = d;
                    closestSocket = socket;
                }
            }
        }

        // 🔹 Si encontramos socket válido
        if (closestSocket != null)
        {
            // ✅ ORDEN CORRECTO: kinematic ANTES de SetParent
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Guardar velocidad del padre (ej: jugador) antes del SetParent
            // para que el snap no frene su movimiento
            Rigidbody parentRb = closestSocket.GetComponentInParent<Rigidbody>();
            Vector3 savedVelocity = Vector3.zero;
            Vector3 savedAngular  = Vector3.zero;
            if (parentRb != null && parentRb != rb)
            {
                savedVelocity = parentRb.linearVelocity;
                savedAngular  = parentRb.angularVelocity;
            }

            transform.position = closestSocket.transform.position;
            // No sobreescribir la rotación: conservar la que el usuario aplicó
            transform.SetParent(closestSocket.transform, true);

            // Restaurar velocidad del padre tras el SetParent
           /* if (parentRb != null && parentRb != rb)
            {
                parentRb.linearVelocity  = savedVelocity;
                parentRb.angularVelocity = savedAngular;
            }
            */

            closestSocket.isOccupied = true;

            // Activar los sockets propios de esta pieza
            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                if (s.transform != closestSocket.transform)
                {
                    s.gameObject.SetActive(true);
                    s.isOccupied = false;
                }
            }

            attachedNode = closestSocket.transform;
            isAttached = true;
        }
        else
        {
            // No encajó: devolver física normal
            if (rb != null) rb.isKinematic = false;
        }
    }
    // ── Reenvíos desde ChildClickForwarder (avisaCollider) ───────────────────
    public void OnChildMouseDown() => OnMouseDown();
    public void OnChildMouseDrag() => OnMouseDrag();
    public void OnChildMouseUp()   => OnMouseUp();

}
