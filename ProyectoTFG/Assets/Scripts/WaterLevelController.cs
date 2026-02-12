using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
    [Header("Puzzle Components")]
    public PuzzleButton button1;
    public PuzzleButton button2;
    public Transform waterSurface;

    [Header("Settings")]
    public float targetY = 0f; // Altura objetivo (suelo)
    public float moveSpeed = 2f;
    public bool returnToOriginalHeight = true; // Si vuelve a subir cuando se sueltan los botones

    private float initialY;
    private bool isSolved = false;

    void Start()
    {
        if (waterSurface != null)
        {
            initialY = waterSurface.position.y;
        }
        else
        {
            Debug.LogError("Assign Water Surface in WaterLevelController!");
        }
    }

    void Update()
    {
        if (button1 == null || button2 == null || waterSurface == null) return;

        bool bothPressed = button1.isPressed && button2.isPressed;

        float targetHeight = initialY;

        if (bothPressed)
        {
            targetHeight = targetY;
            isSolved = true; // Mark as solved if you want it to stay down permanently, remove 'returnToOriginalHeight' logic if so.
        }
        else if (!returnToOriginalHeight && isSolved)
        {
             targetHeight = targetY; // Stay down if solved once and return is false
        }
        
        // Mover el agua suavemente
        Vector3 newPos = waterSurface.position;
        newPos.y = Mathf.MoveTowards(newPos.y, targetHeight, moveSpeed * Time.deltaTime);
        waterSurface.position = newPos;
    }
}
