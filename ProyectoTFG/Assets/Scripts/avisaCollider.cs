using UnityEngine;

public class ChildClickForwarder : MonoBehaviour
{
    private DropAndDrag parentScript;

    private void Awake()
    {
        parentScript = GetComponentInParent<DropAndDrag>();
    }

    private void OnMouseDown()
    {
        if (parentScript != null)
            parentScript.OnChildMouseDown();
    }

    private void OnMouseDrag()
    {
        if (parentScript != null)
            parentScript.OnChildMouseDrag();
    }

    private void OnMouseUp()
    {
        if (parentScript != null)
            parentScript.OnChildMouseUp();
    }
}
