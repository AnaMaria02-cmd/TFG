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

    void FixedUpdate()
    {
        // Buscar todas las colisiones dentro del radio de atracción
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
                    // Calcular la dirección hacia el imán
                    Vector3 directionToMagnet = (transform.position - coinRb.transform.position).normalized;
                    
                    // Calcular la distancia para hacer la fuerza mayor cuanto más cerca esté (opcional)
                    float distance = Vector3.Distance(transform.position, coinRb.transform.position);
                    float forceMultiplier = 1f - (distance / attractionRadius); // Más fuerte de cerca

                    // Aplicar la fuerza magnética
                    coinRb.AddForce(directionToMagnet * attractionForce * forceMultiplier, ForceMode.Acceleration);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Cuando una moneda choca físicamente contra el imán
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
}
