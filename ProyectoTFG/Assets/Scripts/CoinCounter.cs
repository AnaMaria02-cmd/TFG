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

    // Variable interna para llevar la cuenta
    private int currentCoins = 0;
    
    // Rastrear monedas para no contarlas múltiples veces si rebotan
    private HashSet<GameObject> countedCoins = new HashSet<GameObject>();

    private void Start()
    {
        UpdateUI();
    }

    // Este método se llama automáticamente cuando otro objeto con Collider choca con este
    // REQUISITO: Este objeto debe tener el Collider en modo "Is Trigger" marcado.
    private void OnTriggerEnter(Collider other)
    {
        // Comprobar si el objeto que acaba de entrar tiene el tag que buscamos y no ha sido contado antes
        if (other.CompareTag(coinTag))
        {
            if (!countedCoins.Contains(other.gameObject))
            {
                countedCoins.Add(other.gameObject); // Lo marcamos como contado
                currentCoins++; // Sumamos una moneda a la cuenta
                UpdateUI();     // Actualizamos el texto en pantalla
                
                // Opcional: Limpiar referencias nulas de vez en cuando si se acumulan referenciando monedas borradas en rondas pasadas
                if (countedCoins.Count > 500) countedCoins.RemoveWhere(c => c == null);
                
                Debug.Log($"¡Moneda recolectada! Total: {currentCoins}");
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
