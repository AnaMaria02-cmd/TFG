using UnityEngine;

public class Socket : MonoBehaviour
{
    public bool isOccupied = false;

    private void OnEnable()
    {
        if (SocketManager.Instance != null)
        {
            SocketManager.Instance.RegisterSocket(this);
        }
    }

    private void OnDisable()
    {
        if (SocketManager.Instance != null)
        {
            SocketManager.Instance.UnregisterSocket(this);
        }
    }

    private void Start()
    {
        // Ensure registration if Manager wasn't ready during OnEnable
        if (SocketManager.Instance != null)
        {
            SocketManager.Instance.RegisterSocket(this);
        }
    }
}