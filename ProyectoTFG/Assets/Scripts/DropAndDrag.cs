using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class DropAndDrag : MonoBehaviour
{
    Vector3 mouseOffset;
    Vector3 worldPosition;

    public float snapDistance = 1f;
    public List<Transform> nodes = new List<Transform>();
    public Transform playerTransform;

    bool isAttached = false;
    Transform attachedNode;

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDown()
    {
        // Si está ensamblada → desensamblar
        if (isAttached)
        {
            // Liberar socket anterior
            Socket socket = attachedNode.GetComponent<Socket>();
            if (socket != null)
                socket.isOccupied = false;

            // Desparentar
            transform.SetParent(null, true);

            // Desactivar todos los sockets de la pieza
            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                s.gameObject.SetActive(false);
                s.isOccupied = false;
            }

            // Reset estado
            attachedNode = null;
            isAttached = false;
        }

        // Calcular offset para drag
        mouseOffset = transform.position - GetMouseWorldPos();
    }



    private void OnMouseDrag()
    {
        if (isAttached) return;

        worldPosition = GetMouseWorldPos() + mouseOffset;
        transform.position = worldPosition;
    }

    private void OnMouseUp()
    {
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

            // Marcar socket pegado como ocupado
            closestSocket.isOccupied = true;

            // Activar todos los sockets de la nueva pieza
            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                if (s.transform != closestSocket.transform) // no reactivar el socket donde pegaste
                {
                    Debug.Log("activando");
                    s.gameObject.SetActive(true);
                    s.isOccupied = false;
                }
            }

            // Estado de la pieza
            attachedNode = closestSocket.transform;
            isAttached = true;
        }

    }

}

