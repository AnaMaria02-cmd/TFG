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
    [Tooltip("Selecciona aquí la capa (ej. 'lata') para que el imán solo atraiga esos objetos.")]
    public LayerMask capasAtraibles = ~0; // ~0 significa "Todo" por defecto para no romper el script de golpe
    public bool activado = true;

    void FixedUpdate()
    {
        if (!activado) return;

        // Utilizamos el layerMask aquí para que Unity solo detecte objetos en esa capa
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attractionRadius, capasAtraibles);
        foreach (var hitCollider in hitColliders)
        {
            // Usar attachedRigidbody es más seguro por si el Rigidbody está en un padre
            Rigidbody coinRb = hitCollider.attachedRigidbody; 

            // Comprobar si el objeto o su rigidbody padre tienen el tag
            bool hasTag = hitCollider.CompareTag(coinTag);
            if (!hasTag && coinRb != null)
            {
                hasTag = coinRb.gameObject.CompareTag(coinTag);
            }

            if (hasTag)
            {
                // Solo atraemos si tiene Rigidbody y no es cinemático
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
        // Comprobar primero que el objeto con el que colisiona esté en la capa correcta
        bool isCorrectLayer = ((1 << collision.gameObject.layer) & capasAtraibles) != 0;

        if (!isCorrectLayer) return;

        Rigidbody coinRb = collision.rigidbody; 
        bool hasTag = collision.gameObject.CompareTag(coinTag);
        if (!hasTag && coinRb != null)
        {
            hasTag = coinRb.gameObject.CompareTag(coinTag);
        }

        if (hasTag)
        {
            if (coinRb != null)
            {
                // Desactivar físicas en la moneda para que deje de moverse y se quede pegada
                coinRb.linearVelocity = Vector3.zero;
                coinRb.angularVelocity = Vector3.zero;
                coinRb.isKinematic = true;
            }

            // Hacer que el objeto raíz sea hijo del imán
            Transform rootToAttach = (coinRb != null) ? coinRb.transform : collision.transform;
            rootToAttach.SetParent(this.transform);
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
            
            Rigidbody childRb = child.GetComponent<Rigidbody>();
            bool hasTag = child.CompareTag(coinTag);
            
            if (hasTag)
            {
                child.SetParent(null);
                if (childRb != null)
                {
                    childRb.isKinematic = false;
                }
            }
        }
    }
}
