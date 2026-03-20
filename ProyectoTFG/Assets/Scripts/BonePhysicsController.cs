using UnityEngine;

public class BonePhysicsController : MonoBehaviour
{
    private Rigidbody[] boneRigidbodies;

    void Start()
    {
        // Busca automáticamente todos los componentes Rigidbody en la pieza y en sus hijos (los huesos)
        boneRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    // 👉 Asigna esta función en el evento "OnClick" de tu Botón
    public void ActivarFisicas()
    {
        foreach (Rigidbody rb in boneRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }

    // 👉 (Opcional) Usa esta función si necesitas otro botón para volver a bloquearlos
    public void DesactivarFisicas()
    {
        foreach (Rigidbody rb in boneRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                
                // Quitamos la inercia para que se detengan de inmediato
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    // 👉 (Opcional) Usa esta función si quieres que el mismo botón alterne entre rígido y suelto
    public void AlternarFisicas()
    {
        foreach (Rigidbody rb in boneRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = !rb.isKinematic;
                
                if (rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
