using UnityEngine;

public class AttachablePiece : MonoBehaviour
{
    private bool isSelected = false;
    private Transform attachPoint;

    void Update()
    {
        if (isSelected)
        {
            // Mover con el mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                transform.position = hit.point;
            }

            // Rotar con Q/E
            if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.up, -90 * Time.deltaTime);
            if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, 90 * Time.deltaTime);
        }
    }

    private void OnMouseDown()
    {
        isSelected = true;
    }

    private void OnMouseUp()
    {
        isSelected = false;

        // Buscar AttachPoints cercanos
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("AttachPoint"))
            {
                // Snap al attach point
                transform.position = col.transform.position;
                transform.rotation = col.transform.rotation;
                transform.SetParent(col.transform.parent); // se engancha al Body
                break;
            }
        }
    }
}
