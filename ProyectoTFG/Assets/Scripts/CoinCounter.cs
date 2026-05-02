using UnityEngine;
using TMPro; 
using System.Collections;
using System.Collections.Generic;

public class CoinCounter : MonoBehaviour
{
    [Header("Configuración de Monedas")]
    [Tooltip("El tag que tienen tus monedas (por ejemplo: 'Coin', 'Moneda', etc.)")]
    public string coinTag = "Coin";
    
    [Header("Mejoras de Tienda")]
    [Tooltip("El multiplicador actual (1 por defecto, 2 si se compra la mejora)")]
    public int coinMultiplier = 1;
    
    [Header("Interfaz de Usuario (UI)")]
    [Tooltip("Arrastra aquí el objeto de texto (TextMeshPro o Text) del Canvas que mostrará el contador")]
    public TextMeshProUGUI coinUIText; 

    [Tooltip("Panel que se mostrará al ganar la partida")]
    public GameObject victoryPanel;

    [Tooltip("Cantidad de monedas necesarias para ganar")]
    public int coinsToWin = 100;

    [Tooltip("Nombre de la escena del menú inicial (para el botón de salir)")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Efectos de Sonido")]
    [Tooltip("Sonido que se reproducirá al recoger una lata/basura")]
    public AudioClip sonidoLata;
    [Tooltip("Sonido que se reproducirá al recoger un no conductor")]
    public AudioClip sonidoNoConductor;
    
    [Tooltip("El tag para los objetos no conductores")]
    public string nonConductorTag = "NoConductor";

    [Header("Feedback Visual")]
    [Tooltip("La luz que parpadeará al recoger un objeto")]
    public Light feedbackLight;
    [Tooltip("Intensidad máxima a la que llegará la luz")]
    public float maxLightIntensity = 5f;
    [Tooltip("Velocidad del parpadeo (fade in corto)")]
    public float lightFadeSpeed = 15f;
    private Coroutine flashCoroutine;

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
                currentCoins += coinMultiplier; 
                UpdateUI();     
                
                TriggerLightFeedback();
                
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
                
                TriggerLightFeedback();

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

        // La victoria ahora se activa manualmente comprando en la tienda
    }

    public void TriggerVictory()
    {
        if (victoryPanel != null && !victoryPanel.activeSelf)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa el juego
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (victoryPanel == null)
        {
            Debug.LogWarning("¡Has ganado pero no hay panel de victoria asignado!");
        }
    }

    private void TriggerLightFeedback()
    {
        if (feedbackLight != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashLight());
        }
    }

    private IEnumerator FlashLight()
    {
        if (feedbackLight == null) yield break;

        // Fase 1: Fade in muy rápido
        float currentIntensity = feedbackLight.intensity;
        while (currentIntensity < maxLightIntensity)
        {
            currentIntensity += Time.deltaTime * lightFadeSpeed * maxLightIntensity;
            feedbackLight.intensity = currentIntensity;
            yield return null;
        }
        feedbackLight.intensity = maxLightIntensity;

        // Breve pausa arriba para que se note el flash
        yield return new WaitForSeconds(0.05f);

        // Fase 2: Fade out un poco más suave
        while (currentIntensity > 0)
        {
            currentIntensity -= Time.deltaTime * lightFadeSpeed * maxLightIntensity * 0.5f;
            feedbackLight.intensity = Mathf.Max(0, currentIntensity);
            yield return null;
        }
        feedbackLight.intensity = 0f;
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

    public void EnableDoubleMoney()
    {
        // Puedes hacer que se acumule (coinMultiplier *= 2) o que se ponga fijo en 2 (coinMultiplier = 2)
        // Por ahora lo ponemos en 2 como "ganar por dos"
        coinMultiplier = 2;
        Debug.Log("¡Multiplicador x2 Activado!");
    }

    public void ResetMultiplier()
    {
        coinMultiplier = 1;
        Debug.Log("Multiplicador devuelto a x1");
    }

    // ── MÉTODOS PARA LOS BOTONES DE VICTORIA ──

    public void RestartGame()
    {
        // Restablece el tiempo para que el juego no se quede pausado
        Time.timeScale = 1f;
        // Recarga la escena actual (restablece monedas, tiempo, todo en general)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // Restablece el tiempo
        Time.timeScale = 1f;
        // Carga la pantalla inicial (asegúrate de que esté en el Build Settings)
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
