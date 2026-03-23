using UnityEngine;
using TMPro;

public class TimerAndShopManager : MonoBehaviour
{
    public static TimerAndShopManager Instance;

    [Header("Configuración del Tiempo")]
    public float timeRemaining = 60f; // 1 minuto
    public TextMeshProUGUI timerText; // Arrastrar el Texto del Cronómetro aquí
    private bool isTimerRunning = true;

    [Header("Configuración de la Tienda")]
    public GameObject shopPanel;      // Arrastrar el Panel completo de la tienda aquí
    public TextMeshProUGUI moneyText; // Arrastrar el Texto donde dice "Dinero: X" aquí
    
    [Header("Referencias")]
    [Tooltip("Arrastra el objeto que tiene el script CoinCounter (el hueco) para leer el dinero")]
    public CoinCounter coinCounter; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Asegurarnos de que la tienda empiece cerrada y el tiempo corra normal
        if (shopPanel != null) shopPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                // ¡El tiempo se ha acabado!
                timeRemaining = 0;
                isTimerRunning = false;
                UpdateTimerUI();
                OpenShop();
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // ── LÓGICA DE LA TIENDA ──

    private void OpenShop()
    {
        // Pausar el juego
        Time.timeScale = 0f; 
        
        // Mostrar el panel de la tienda
        if (shopPanel != null) shopPanel.SetActive(true);

        // Actualizar el texto del dinero
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null && coinCounter != null)
        {
            moneyText.text = "Dinero: " + coinCounter.GetCurrentCoins().ToString();
        }
    }

    // ── MÉTODO SEGURO PARA COMPRAR DESDE OTROS SCRIPTS (Ej: ShopItem) ──
    public bool TrySpendMoney(int amount)
    {
        if (coinCounter != null && coinCounter.GetCurrentCoins() >= amount)
        {
            coinCounter.SpendCoins(amount);
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    // ── ESTOS MÉTODOS ERAN PARA LOS BOTONES ANTIGUOS (Opcional mantenerlos) ──

    // Arrastra el botón a OnClick, pon este script y selecciona BuyPiece. 
    // En el hueco que aparece, escribe cuánto cuesta (ej. 10)
    public void BuyPiece(int cost)
    {
        if (coinCounter != null && coinCounter.GetCurrentCoins() >= cost)
        {
            coinCounter.SpendCoins(cost); // Descontar el dinero
            UpdateMoneyUI();              // Actualizar texto
            
            Debug.Log($"¡Has comprado una PIEZA por {cost} monedas!");
            
            // TODO: Aquí puedes instanciar / spawnear la nueva pieza en la escena
        }
        else
        {
            Debug.LogWarning("No tienes suficiente dinero para comprar esta pieza.");
        }
    }

    // Usar igual que BuyPiece, para habilidades
    public void BuyAbility(int cost)
    {
        if (coinCounter != null && coinCounter.GetCurrentCoins() >= cost)
        {
            coinCounter.SpendCoins(cost); // Descontar el dinero
            UpdateMoneyUI();              // Actualizar texto
            
            Debug.Log($"¡Has comprado una HABILIDAD por {cost} monedas!");
            
            // TODO: Aquí puedes activar la habilidad en tu PlayerMovement o donde corresponda
        }
        else
        {
            Debug.LogWarning("No tienes suficiente dinero para comprar esta habilidad.");
        }
    }

    // Botón para cerrar la tienda y seguir jugando (opcional)
    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        Time.timeScale = 1f; // Reanudar físicas y juego
        isTimerRunning = true; // Si quieres que el tiempo vuelva a empezar, pon timeRemaining = 60f aquí
    }
}
