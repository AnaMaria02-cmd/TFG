using UnityEngine;

public class ChildClickForwarder : MonoBehaviour
{
    private void OnMouseDown()
    {
        var parentScript = GetComponentInParent<DropAndDrag>();
        if (parentScript != null)
        {
            parentScript.OnChildClicked();
        }
    }
}
