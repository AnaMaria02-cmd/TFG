using System.Collections.Generic;
using UnityEngine;

public class DropAndDrag : MonoBehaviour
{
    Vector3 mouseOffset;
    Vector3 worldPosition;

    public float snapDistance = 1f;
    public List<Transform> nodes = new List<Transform>();
    public Transform playerTransform;

    bool isAttached = false;
    bool isSelected = false;
    Transform attachedNode;

    private Plane dragPlane;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // Rotación solo si está seleccionada
        if (isSelected && Input.GetMouseButtonDown(1))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Rotar en eje X
                transform.Rotate(90f, 0f, 0f, Space.World);
            }
            else
            {
                // Rotar en eje Y
                transform.Rotate(0f, 90f, 0f, Space.World);
            }
        }
    }

    private void OnMouseDown()
    {
        isSelected = true;

        // Crear plano perpendicular al eje Z (bloquea movimiento en Z)
        dragPlane = new Plane(Vector3.forward, transform.position);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            mouseOffset = transform.position - ray.GetPoint(distance);
        }

        // Si está ensamblada → desensamblar
        if (isAttached)
        {
            Socket socket = attachedNode.GetComponent<Socket>();
            if (socket != null)
                socket.isOccupied = false;

            transform.SetParent(null, true);

            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                s.gameObject.SetActive(false);
                s.isOccupied = false;
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

        if (isAttached) return;

        Socket closestSocket = null;
        float smallestDistance = snapDistance;

        foreach (Socket socket in FindObjectsOfType<Socket>())
        {
            if (socket.isOccupied) continue;

            float d = Vector3.Distance(socket.transform.position, worldPosition);
            if (d < smallestDistance)
            {
                smallestDistance = d;
                closestSocket = socket;
            }
        }

        if (closestSocket != null)
        {
            // Snap exacto
            transform.position = closestSocket.transform.position;
            transform.rotation = closestSocket.transform.rotation;

            // Parent correcto
            transform.SetParent(closestSocket.transform, true);

            // Marcar socket como ocupado
            closestSocket.isOccupied = true;

            // Activar sockets hijos
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
    }
}
