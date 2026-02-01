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
            transform.SetParent(null, true);
            attachedNode = null;
            isAttached = false;
        }

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

        float smallestDistance = snapDistance;
        Transform closestNode = null;

        foreach (Transform node in nodes)
        {
            float distance = Vector3.Distance(node.position, worldPosition);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestNode = node;
            }
        }

        if (closestNode != null)
        {
            // Snap exacto
            transform.position = closestNode.position;
            transform.rotation = closestNode.rotation;

            // Parentado CORRECTO
            transform.SetParent(closestNode, true);

            attachedNode = closestNode;
            isAttached = true;
        }
    }
}
