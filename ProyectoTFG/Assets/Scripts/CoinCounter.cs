using UnityEngine;
using TMPro; // Usamos TextMeshPro porque es el estándar moderno en Unity. Cámbiarlo a "using UnityEngine.UI" y "Text" si usas el clásico.

public class CoinCounter : MonoBehaviour
{
    [Header("Configuración de Monedas")]
    [Tooltip("El tag que tienen tus monedas (por ejemplo: 'Coin', 'Moneda', etc.)")]
    public string coinTag = "Coin";
    
    [Header("Interfaz de Usuario (UI)")]
    [Tooltip("Arrastra aquí el objeto de texto (TextMeshPro o Text) del Canvas que mostrará el contador")]
    public TextMeshProUGUI coinUIText; 

    // Variable interna para llevar la cuenta
    private int currentCoins = 0;

    private void Start()
    {
        UpdateUI();
    }

    // Este método se llama automáticamente cuando otro objeto con Collider choca con este
    // REQUISITO: Este objeto debe tener el Collider en modo "Is Trigger" marcado.
    private void OnTriggerEnter(Collider other)
    {
        // Comprobar si el objeto que acaba de entrar tiene el tag que buscamos
        if (other.CompareTag(coinTag))
        {
            currentCoins++; // Sumamos una moneda a la cuenta
            UpdateUI();     // Actualizamos el texto en pantalla
            
            // Opcional: si quieres que la moneda desaparezca al caer
            // Destroy(other.gameObject); 
            
            Debug.Log($"¡Moneda recolectada! Total: {currentCoins}");
        }
    }

    // Opcional: Si quieres que reste monedas si por algún motivo las sacan del hueco
    private void OnTriggerExit(Collider other)
    {
        /*
        if (other.CompareTag(coinTag))
        {
            currentCoins--;
            UpdateUI();
        }
        */
    }

    private void UpdateUI()
    {
        if (coinUIText != null)
        {
            coinUIText.text = "Monedas: " + currentCoins.ToString();
        }
        else
        {
            Debug.LogWarning("[CoinCounter] No has asignado el componente de Texto (UI) en el inspector.");
        }
    }
}
