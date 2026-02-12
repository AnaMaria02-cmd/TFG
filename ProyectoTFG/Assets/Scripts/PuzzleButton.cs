using UnityEngine;

public class PuzzleButton : MonoBehaviour
{
    public bool isPressed = false;
    public Transform buttonTop;
    public float pressDistance = 0.1f;
    public float pressSpeed = 5f;
    public Transform playerTransform;


    private Vector3 initialPos;
    private int objectsOnButton = 0;

    void Start()
    {
        if (buttonTop != null)
        {
            initialPos = buttonTop.localPosition;
        }
    }

    void Update()
    {
        if (buttonTop != null)
        {
            Vector3 targetPos = isPressed ? initialPos - Vector3.up * pressDistance : initialPos;
            buttonTop.localPosition = Vector3.Lerp(buttonTop.localPosition, targetPos, Time.deltaTime * pressSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if other is a trigger to avoid duplicate events
        if (other.isTrigger) return;

        // Check if object is Player or attached to Player
        if (IsValidObject(other))
        {
            objectsOnButton++;
            UpdateState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        if (IsValidObject(other))
        {
            objectsOnButton--;
            if (objectsOnButton < 0) objectsOnButton = 0;
            UpdateState();
        }
    }

    private bool IsValidObject(Collider other)
    {
        // Check if object is tagged as Player directly
        if (other.CompareTag("Player")) return true;

        // Check if it's an attachable piece attached to the player (root is player)
        // Note: Assuming Player object has tag "Player"
        DropAndDrag piece = other.GetComponent<DropAndDrag>();
        if (piece != null && other.transform.IsChildOf(playerTransform))
        {
            return true;
        }

        return false;
    }

    private void UpdateState()
    {
        isPressed = objectsOnButton > 0;
    }
}
