using UnityEngine;
using TMPro; 
using System.Collections.Generic;

public class CoinCounter : MonoBehaviour
{
    [Header("Configuración de Monedas")]
    [Tooltip("El tag que tienen tus monedas (por ejemplo: 'Coin', 'Moneda', etc.)")]
    public string coinTag = "Coin";
    
    [Header("Interfaz de Usuario (UI)")]
    [Tooltip("Arrastra aquí el objeto de texto (TextMeshPro o Text) del Canvas que mostrará el contador")]
    public TextMeshProUGUI coinUIText; 

    [Header("Efectos de Sonido")]
    [Tooltip("Sonido que se reproducirá al recoger una lata/basura")]
    public AudioClip sonidoLata;
    [Tooltip("Sonido que se reproducirá al recoger un no conductor")]
    public AudioClip sonidoNoConductor;
    
    [Tooltip("El tag para los objetos no conductores")]
    public string nonConductorTag = "NoConductor";

    // ... (rest of Start and vars same)
    private int currentCoins = 0;
    private HashSet<GameObject> countedCoins = new HashSet<GameObject>();

    private void Start()
    {
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobar si es lata (moneda)
        if (other.CompareTag(coinTag))
        {
            if (!countedCoins.Contains(other.gameObject))
            {
                countedCoins.Add(other.gameObject); 
                currentCoins++; 
                UpdateUI();     
                
                if (sonidoLata != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoLata, transform.position);
                }
                Destroy(other.gameObject); // Destruir la lata para que desaparezca

                if (countedCoins.Count > 500) countedCoins.RemoveWhere(c => c == null);
            }
        }
        // Comprobar si es no conductor
        else if (other.CompareTag(nonConductorTag))
        {
            if (!countedCoins.Contains(other.gameObject))
            {
                countedCoins.Add(other.gameObject); 
                // Asumo que da puntos o no, aquí lo tratamos solo para sonido por si acaso
                // currentCoins++; // Descomenta si también te da puntos
                // UpdateUI();
                
                if (sonidoNoConductor != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoNoConductor, transform.position);
                }
                Destroy(other.gameObject); // Destruir el no conductor para que desaparezca

                if (countedCoins.Count > 500) countedCoins.RemoveWhere(c => c == null);
            }
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
    }

    // ── MÉTODOS PÚBLICOS PARA LA TIENDA ──

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public void SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateUI();
        }
    }
}
