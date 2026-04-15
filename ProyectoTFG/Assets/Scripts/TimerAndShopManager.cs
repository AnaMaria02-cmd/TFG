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

    [Header("Incremento y UI de Precios")]
    [Tooltip("Por cuánto se multiplica el precio cada vez que compras")]
    public float multiplicadorPrecio = 1.5f;
    public TextMeshProUGUI textoPrecioLarga;
    public TextMeshProUGUI textoPrecioBlanda;
    public TextMeshProUGUI textoPrecioIman;

    [Header("Avisos de la Tienda")]
    [Tooltip("La Imagen del panel de aviso de 'Sin dinero' (arrastra aquí la imagen)")]
    public UnityEngine.UI.Image panelAvisoDinero;
    public float tiempoMostrarAviso = 1.5f;
    public float tiempoFadeOut = 1f;
    private Coroutine rutinaAvisoDinero;

    [Header("Configuración del Inventario")]
    public GameObject inventoryPanel; // Arrastrar el panel del Inventario (edición) aquí
    
    [Header("Reset de Escenario e Inicio")]
    [Tooltip("El Prefab que contiene TODAS las pelotas/monedas juntas")]
    public GameObject prefabPelotasEscenario;
    [Tooltip("El Transform de tu Jugador")]
    public Transform jugadorTransform;
    [Tooltip("Un GameObject vacío colodado donde el jugador debe reaparecer siempre")]
    public Transform jugadorSpawnPoint;
    
    // Rastreador interno de las pelotas actualmente en el mapa
    private GameObject instanciaPelotasActual;

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
        
        // Al empezar la partida también spawneamos las pelotas la primera vez
        SpawnearPelotas();
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
                
                if (InventoryManager.Instance != null) 
                {
                    InventoryManager.Instance.EncenderTodosImanes();
                }

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

        // Actualizar el texto del dinero y precios
        UpdateMoneyUI();
        ActualizarTextosPrecios();
    }

    private void ActualizarTextosPrecios()
    {
        if (textoPrecioLarga != null) textoPrecioLarga.text = costeLarga.ToString();
        if (textoPrecioBlanda != null) textoPrecioBlanda.text = costeBlanda.ToString();
        if (textoPrecioIman != null) textoPrecioIman.text = costeIman.ToString();
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
            if (InventoryManager.Instance != null) 
            {
                InventoryManager.Instance.AddPieceToInventory(null, "Pieza Larga");
                costeLarga = Mathf.RoundToInt(costeLarga * multiplicadorPrecio);
                ActualizarTextosPrecios();
            }
            else Debug.LogWarning("¡Te falta el InventoryManager en tu escena para guardar la pieza!");
        }
        else 
        {
            Debug.LogWarning("No tienes dinero suficiente para la Pieza Larga");
            MostrarAvisoDinero();
        }
    }

    public void ComprarPiezaBlanda()
    {
        if (TrySpendMoney(costeBlanda))
        {
            if (InventoryManager.Instance != null) 
            {
                InventoryManager.Instance.AddPieceToInventory(null, "Pieza Blanda");
                costeBlanda = Mathf.RoundToInt(costeBlanda * multiplicadorPrecio);
                ActualizarTextosPrecios();
            }
            else Debug.LogWarning("¡Te falta el InventoryManager en tu escena para guardar la pieza!");
        }
        else 
        {
            Debug.LogWarning("No tienes dinero suficiente para la Pieza Blanda");
            MostrarAvisoDinero();
        }
    }

    public void ComprarIman()
    {
        if (TrySpendMoney(costeIman))
        {
            if (InventoryManager.Instance != null) 
            {
                InventoryManager.Instance.AddPieceToInventory(null, "Iman");
                costeIman = Mathf.RoundToInt(costeIman * multiplicadorPrecio);
                ActualizarTextosPrecios();
            }
            else Debug.LogWarning("¡Te falta el InventoryManager en tu escena para guardar la pieza!");
        }
        else 
        {
            Debug.LogWarning("No tienes dinero suficiente para el Iman");
            MostrarAvisoDinero();
        }
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
        
        // LIMPIEZA DE MAPA
        // Destruir las pelotas antiguas que hubiese en pantalla
        if (instanciaPelotasActual != null) 
        {
            Destroy(instanciaPelotasActual);
            instanciaPelotasActual = null;
        }
        else 
        {
            // Opcional: Si el usuario había puesto pelotas a mano en la escena y es la primera vez,
            // podemos buscarlas por tag y eliminarlas. Añade tag "Moneda" o "Pelota" a esos objetos en el futuro si lo necesitas.
            GameObject[] pelotasManuales = GameObject.FindGameObjectsWithTag("Coin"); // Usar el tag correcto si hace falta
            foreach(GameObject p in pelotasManuales) Destroy(p);
        }

        // MOVER AL JUGADOR AL INICIO DE LA PANTALLA
        if (jugadorTransform != null && jugadorSpawnPoint != null)
        {
            // Apagamos físicas momentáneamente si usa un CharacterController o Rigidbody especial
            // Aunque mover por Transform a veces funciona directo
            jugadorTransform.position = jugadorSpawnPoint.position;
            jugadorTransform.rotation = jugadorSpawnPoint.rotation;
        }

        // El cronómetro sigue parado mientras el jugador se edita y elige piezas
        Time.timeScale = 1f; // Permitimos físicas para que el jugador pueda interactuar o editar sus piezas
        isTimerRunning = false;
    }

    // Método interno para generar todas las pelotas otra vez
    private void SpawnearPelotas()
    {
        if (prefabPelotasEscenario != null)
        {
            // Por precaución, si ya había unas instanciadas, las volvemos a borrar
            if (instanciaPelotasActual != null) Destroy(instanciaPelotasActual);

            // Las instanciamos en la coordenada 0,0,0 pensando en que el Prefab ya las tiene en sus posiciones del nivel correctas
            instanciaPelotasActual = Instantiate(prefabPelotasEscenario, Vector3.zero, Quaternion.identity);
        }
    }

    // Asignar al botón "Play"/Jugar dentro del inventario
    public void StartGame()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        // MOVER AL JUGADOR AL INICIO DE LA PANTALLA
        if (jugadorTransform != null && jugadorSpawnPoint != null)
        {
            // Apagamos físicas momentáneamente si usa un CharacterController o Rigidbody especial
            // Aunque mover por Transform a veces funciona directo
            jugadorTransform.position = jugadorSpawnPoint.position;
            jugadorTransform.rotation = jugadorSpawnPoint.rotation;
        }

        // RECARGAR EL NIVEL GENERANDO TODAS LAS PELOTAS
        SpawnearPelotas();

        // Empieza el cronómetro de (ej. 60 segundos) y se reanuda el bucle del juego
        timeRemaining = 60f;
        isTimerRunning = true;
        Time.timeScale = 1f; // Asegurarse de que las físicas continúan su estado normal
    }

    // ── GESTIÓN DE AVISOS ──
    private void MostrarAvisoDinero()
    {
        if (panelAvisoDinero != null)
        {
            if (rutinaAvisoDinero != null) StopCoroutine(rutinaAvisoDinero);
            rutinaAvisoDinero = StartCoroutine(RutinaMostrarYDesvanecerAviso());
        }
        else
        {
            Debug.LogWarning("Configura el panelAvisoDinero en el Inspector para ver el mensaje en pantalla");
        }
    }

    private System.Collections.IEnumerator RutinaMostrarYDesvanecerAviso()
    {
        // Mostrar de golpe
        panelAvisoDinero.gameObject.SetActive(true);
        Color colorFondo = panelAvisoDinero.color;
        colorFondo.a = 1f;
        panelAvisoDinero.color = colorFondo;

        // Esperar (usamos WaitForSecondsRealtime porque el juego está pausado con timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempoMostrarAviso);

        // Fade out (usamos unscaledDeltaTime por la misma razón)
        float tiempo = 0f;
        while (tiempo < tiempoFadeOut)
        {
            tiempo += Time.unscaledDeltaTime;
            colorFondo.a = Mathf.Lerp(1f, 0f, tiempo / tiempoFadeOut);
            panelAvisoDinero.color = colorFondo;
            yield return null;
        }

        colorFondo.a = 0f;
        panelAvisoDinero.color = colorFondo;
        panelAvisoDinero.gameObject.SetActive(false);
    }
}
