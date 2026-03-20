using UnityEngine;
using System.Collections.Generic;

public class Magnet : MonoBehaviour
{
    [Header("Configuración del Imán")]
    [Tooltip("El radio dentro del cual las monedas serán atraídas.")]
    public float attractionRadius = 5f;
    [Tooltip("La fuerza con la que el imán atrae a las monedas.")]
    public float attractionForce = 20f;
    [Tooltip("El tag que deben tener las monedas para ser afectadas.")]
    public string coinTag = "Coin";
    public bool activado = true;

    void FixedUpdate()
    {
        if (!activado) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attractionRadius);
        foreach (var hitCollider in hitColliders)
        {
            // Comprobar si el objeto encontrado es una moneda según su Tag
            if (hitCollider.CompareTag(coinTag))
            {
                Debug.Log("Moneda encontrada");
                Rigidbody coinRb = hitCollider.GetComponent<Rigidbody>();

                // Solo atraemos si tiene Rigidbody y no es cinemático (ya que si lo es, ya está pegado)
                if (coinRb != null && !coinRb.isKinematic)
                {
                    Vector3 delta = transform.position - coinRb.transform.position;
                    float distance = delta.magnitude;

                    // Evitamos errores si la distancia es casi cero
                    if (distance > 0.1f)
                    {
                        Vector3 directionToMagnet = delta.normalized;
                        // Usamos Clamp01 para evitar valores negativos
                        float forceMultiplier = Mathf.Clamp01(1f - (distance / attractionRadius));

                        // Aplicamos la fuerza
                        coinRb.AddForce(directionToMagnet * attractionForce * forceMultiplier, ForceMode.Acceleration);

                        // Opcional: Aplicar un poco de "Drag" artificial para que no lleguen con demasiada velocidad
                        coinRb.linearVelocity *= 0.95f;
                    }
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(coinTag))
        {
            Rigidbody coinRb = collision.gameObject.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                // Desactivar físicas en la moneda para que deje de moverse y se quede pegada
                coinRb.isKinematic = true;
                coinRb.linearVelocity = Vector3.zero;
                coinRb.angularVelocity = Vector3.zero;
            }

            // Hacer que la moneda sea hija del imán, así se moverá junto con él
            collision.transform.SetParent(this.transform);
        }
    }

    // Dibuja una esfera azul en el editor para visualizar el área de atracción del imán
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, attractionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
    
    public void cambiarFuerza()
    {
        if (activado)
        {
            activado = false;
            SoltarMonedas();
        }
        else
        {
            activado = true;
        }
    }

    private void SoltarMonedas()
    {
        // Iteramos hacia atrás porque vamos a quitarles el parent
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            
            if (child.CompareTag(coinTag))
            {
                child.SetParent(null);
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }
        }
    }
}
