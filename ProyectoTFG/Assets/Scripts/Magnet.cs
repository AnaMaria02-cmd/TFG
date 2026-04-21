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

    [Header("Sonido")]
    [Tooltip("El sonido por defecto al atraer un objeto.")]
    public AudioClip attachSound;
    [Tooltip("El sonido si el objeto contiene 'Bottle' en su nombre.")]
    public AudioClip bottleSound;
    [Tooltip("El sonido si el objeto contiene 'Pizza' en su nombre.")]
    public AudioClip pizzaSound;
    [Tooltip("El sonido si el objeto contiene 'Paper' o 'Papel' en su nombre.")]
    public AudioClip paperSound;
    [Tooltip("Volumen del sonido (1 es el normal, puedes subirlo).")]
    [Range(0f, 3f)]
    public float attachVolume = 1f;
    private AudioSource audioSource;

    private List<GameObject> atraidas = new List<GameObject>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Hacemos que sea sonido 2D (0f) para que se escuche al máximo volumen sin importar lo lejos que esté la cámara
        audioSource.spatialBlend = 0f; 
    }

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
                // EL TRUCO DEFINITIVO: Destruimos el Rigidbody mientras está pegado.
                // Al no tener Rigidbody, la lata pasa a ser un simple modelo 3D (geometría visual),
                // por lo que Unity no calculará NINGUNA física para ella y jamás saldrá volando por bugs de escala o rotación.
                
                Collider[] colliders = coinRb.GetComponentsInChildren<Collider>();
                foreach(Collider col in colliders)
                {
                    // Lo convertimos en Trigger en vez de apagarlo. Así la física no hace bugs, pero la caja invisible SÍ lo detecta.
                    col.isTrigger = true;
                }

                if (!atraidas.Contains(coinRb.gameObject))
                {
                    atraidas.Add(coinRb.gameObject);

                    if (audioSource != null)
                    {
                        AudioClip clipToPlay = attachSound; // Sonido por defecto

                        // Comprobar el nombre del objeto para elegir el sonido
                        string objectName = coinRb.gameObject.name;
                        
                        // Utilizamos Contains para que coincida aunque se llame "Bottle(Clone)"
                        if (objectName.Contains("Bottle"))
                        {
                            if (bottleSound != null) clipToPlay = bottleSound;
                        }
                        else if (objectName.Contains("Pizza"))
                        {
                            if (pizzaSound != null) clipToPlay = pizzaSound;
                        }
                        else if (objectName.Contains("Paper") || objectName.Contains("Papel"))
                        {
                            if (paperSound != null) clipToPlay = paperSound;
                        }

                        if (clipToPlay != null)
                        {
                            audioSource.PlayOneShot(clipToPlay, attachVolume);
                        }
                    }
                }

                Destroy(coinRb); // Eliminamos la física completamente
            }

            // Hacer que el objeto raíz sea hijo del imán
            Transform rootToAttach = collision.transform; 
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
        foreach (GameObject obj in atraidas)
        {
            if (obj != null)
            {
                obj.transform.SetParent(null);
                
                // Asegurarnos de encender sus colliders otra vez (quitando el trigger)
                Collider[] colliders = obj.GetComponentsInChildren<Collider>();
                foreach(Collider col in colliders)
                {
                    col.isTrigger = false;
                }

                // Devolverle su Rigidbody para que vuelva a caer con las físicas normales
                Rigidbody rbRestaurado = obj.GetComponent<Rigidbody>();
                if (rbRestaurado == null)
                {
                    rbRestaurado = obj.AddComponent<Rigidbody>();
                }
                
                rbRestaurado.isKinematic = false;
                rbRestaurado.WakeUp();
            }
        }
        
        atraidas.Clear();
    }
}
