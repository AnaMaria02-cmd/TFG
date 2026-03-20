using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance { get; private set; }

    private List<Socket> allSockets = new List<Socket>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterSocket(Socket socket)
    {
        if (!allSockets.Contains(socket))
        {
            allSockets.Add(socket);
        }
    }

    public void UnregisterSocket(Socket socket)
    {
        if (allSockets.Contains(socket))
        {
            allSockets.Remove(socket);
        }
    }

    public Socket GetClosestSocket(Vector3 position, float maxDistance, Transform excludeTransform = null)
    {
        Socket closest = null;
        float minDistance = maxDistance;

        foreach (var socket in allSockets)
        {
            if (socket == null || socket.isOccupied || !socket.gameObject.activeInHierarchy) continue;
            
            if (excludeTransform != null && socket.transform.IsChildOf(excludeTransform)) continue;

            float dist = Vector3.Distance(position, socket.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = socket;
            }
        }

        return closest;
    }
}
