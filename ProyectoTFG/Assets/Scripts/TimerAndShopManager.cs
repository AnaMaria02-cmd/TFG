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
    
    [Header("Precios de las 3 Piezas (Ajustables)")]
    public int costeLarga = 5;
    public int costeBlanda = 8;
    public int costeIman = 20;

    [Header("Configuración del Inventario")]
    public GameObject inventoryPanel; // Arrastrar el panel del Inventario (edición) aquí
    
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
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
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

    // ── BOTONES DE COMPRA DIRECTA PARA TUS 3 OBJETOS ──
    
    public void ComprarPiezaLarga()
    {
        if (TrySpendMoney(costeLarga))
        {
            if (InventoryManager.Instance != null) InventoryManager.Instance.AddPieceToInventory(null, "Pieza Larga");
            else Debug.LogWarning("¡Te falta el InventoryManager en tu escena para guardar la pieza!");
        }
        else Debug.LogWarning("No tienes dinero suficiente para la Pieza Larga");
    }

    public void ComprarPiezaBlanda()
    {
        if (TrySpendMoney(costeBlanda))
        {
            if (InventoryManager.Instance != null) InventoryManager.Instance.AddPieceToInventory(null, "Pieza Blanda");
            else Debug.LogWarning("¡Te falta el InventoryManager en tu escena para guardar la pieza!");
        }
        else Debug.LogWarning("No tienes dinero suficiente para la Pieza Blanda");
    }

    public void ComprarIman()
    {
        if (TrySpendMoney(costeIman))
        {
            if (InventoryManager.Instance != null) InventoryManager.Instance.AddPieceToInventory(null, "Iman");
            else Debug.LogWarning("¡Te falta el InventoryManager en tu escena para guardar la pieza!");
        }
        else Debug.LogWarning("No tienes dinero suficiente para el Iman");
    }

    // Botón para cerrar la tienda y seguir jugando (opcional)
    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        Time.timeScale = 1f; // Reanudar físicas y juego
        isTimerRunning = true; // Si quieres que el tiempo vuelva a empezar, pon timeRemaining = 60f aquí
    }

    // ── NAVEGACIÓN ENTRE FASES (Bucle del juego) ──

    // Asignar al botón "Next" dentro de la tienda
    public void GoToInventory()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        
        // El cronómetro sigue parado mientras el jugador se edita y elige piezas
        Time.timeScale = 1f; // Permitimos físicas para que el jugador pueda interactuar o editar sus piezas
        isTimerRunning = false;
    }

    // Asignar al botón "Play"/Jugar dentro del inventario
    public void StartGame()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        
        // Empieza el cronómetro de (ej. 60 segundos) y se reanuda el bucle del juego
        timeRemaining = 60f;
        isTimerRunning = true;
        Time.timeScale = 1f; // Asegurarse de que las físicas continúan su estado normal
    }
}
