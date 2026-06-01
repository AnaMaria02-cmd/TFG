using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Textos de los 3 Botones (Arrastrar TextMeshProUGUI)")]
    public TextMeshProUGUI textoLarga;
    public TextMeshProUGUI textoBlanda;
    public TextMeshProUGUI textoIman;

    [Header("Prefabs de las 3 Piezas (Aparecen al usar)")]
    public GameObject prefabLarga;
    public GameObject prefabBlanda;
    public GameObject prefabIman;

    [Header("Nombres exactos que pusiste en la Tienda")]
    [Tooltip("Deben coincidir exactamente con el Item Name de los ShopItem")]
    public string nombreTiendaLarga = "Pieza Larga";
    public string nombreTiendaBlanda = "Pieza Blanda";
    public string nombreTiendaIman = "Iman";

    [Header("Punto de Spawn")]
    public Transform spawnPoint;

    [Header("Avisos del Inventario")]
    [Tooltip("La Imagen del panel de aviso de 'No hay piezas' (arrastra aquí la imagen)")]
    public UnityEngine.UI.Image panelAvisoPiezas;
    public float tiempoMostrarAviso = 1.5f;
    public float tiempoFadeOut = 1f;
    private Coroutine rutinaAvisoPiezas;

    // Contadores internos
    private int cantidadLarga = 0;
    private int cantidadBlanda = 0;
    private int cantidadIman = 0;

    public int GetCantidadLarga() => cantidadLarga;
    public int GetCantidadBlanda() => cantidadBlanda;
    public int GetCantidadIman() => cantidadIman;

    public void LoadInventoryData(int larga, int blanda, int iman)
    {
        cantidadLarga = larga;
        cantidadBlanda = blanda;
        cantidadIman = iman;
        ActualizarTextos();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        ActualizarTextos();
    }

    // Este método es llamado por ShopItem.cs cuando compras una pieza con éxito
    public void AddPieceToInventory(GameObject piecePrefab, string pieceName)
    {
        // Limpiamos los nombres de espacios vacíos y mayúsculas para que no haya errores tontos al escribir
        string nameComprado = pieceName.Trim().ToLower();
        string nameLarga = nombreTiendaLarga.Trim().ToLower();
        string nameBlanda = nombreTiendaBlanda.Trim().ToLower();
        string nameIman = nombreTiendaIman.Trim().ToLower();

        if (nameComprado == nameLarga) cantidadLarga++;
        else if (nameComprado == nameBlanda) cantidadBlanda++;
        else if (nameComprado == nameIman) cantidadIman++;
        else 
        {
            Debug.LogError($"¡ATENCIÓN! La tienda te ha intentado dar '{pieceName}' pero en tu InventoryManager has definido los nombres como '{nombreTiendaLarga}', '{nombreTiendaBlanda}' y '{nombreTiendaIman}'. ¡Dile a tu ShopItem que se llame de una de esas 3 maneras!");
        }

        ActualizarTextos();
    }

    private void ActualizarTextos()
    {
        if (textoLarga != null) textoLarga.text = $"Larga ({cantidadLarga})";
        if (textoBlanda != null) textoBlanda.text = $"Blanda ({cantidadBlanda})";
        if (textoIman != null) textoIman.text = $"Imán ({cantidadIman})";
    }

    // --- FUNCIONES PARA ASIGNAR EN EL ON CLICK() DE TUS 3 BOTONES MANUALES ---

    public void SpawnPiezaLarga()
    {
        if (cantidadLarga > 0 && prefabLarga != null)
        {
            cantidadLarga--;
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            pos.y = 7f; // Forzar altura 7
            Instantiate(prefabLarga, pos, Quaternion.identity);
            ActualizarTextos();
        }
        else if (cantidadLarga <= 0) 
        {
            Debug.LogWarning("No quedan Piezas Largas");
            MostrarAvisoPiezas();
        }
    }

    public void SpawnPiezaBlanda()
    {
        if (cantidadBlanda > 0 && prefabBlanda != null)
        {
            cantidadBlanda--;
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            pos.y = 7f; // Forzar altura 7
            Instantiate(prefabBlanda, pos, Quaternion.identity);
            ActualizarTextos();
        }
        else if (cantidadBlanda <= 0) 
        {
            Debug.LogWarning("No quedan Piezas Blandas");
            MostrarAvisoPiezas();
        }
    }

    public void SpawnPiezaIman()
    {
        if (cantidadIman > 0 && prefabIman != null)
        {
            cantidadIman--;
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            pos.y = 7f; // Forzar altura 7
            Instantiate(prefabIman, pos, Quaternion.identity);
            ActualizarTextos();
        }
        else if (cantidadIman <= 0) 
        {
            Debug.LogWarning("No quedan Imanes");
            MostrarAvisoPiezas();
        }
    }

    public void ToggleTodosImanes()
    {
        // Buscamos todos los imanes en la escena
        Magnet[] imanes = FindObjectsOfType<Magnet>();
        
        // Iteramos sobre todos y llamamos a la función de activar/desactivar
        foreach(Magnet iman in imanes)
        {
            iman.cambiarFuerza();
        }
        
        Debug.Log($"Se han activado/desactivado un total de {imanes.Length} imanes en la escena.");
    }

    public void EncenderTodosImanes()
    {
        Magnet[] imanes = FindObjectsOfType<Magnet>();
        foreach(Magnet iman in imanes)
        {
            iman.activado = true;
        }
        Debug.Log($"Todos los imanes ({imanes.Length}) han sido encendidos para sincronizarse.");
    }

    public void ToggleTodasPiezasBlandas()
    {
        BonePhysicsController[] piezasBlandas = FindObjectsOfType<BonePhysicsController>();
        foreach (BonePhysicsController pieza in piezasBlandas)
        {
            pieza.AlternarFisicas();
        }
        Debug.Log($"Se han activado/desactivado las físicas de {piezasBlandas.Length} piezas blandas en la escena.");
    }

    public void ActivarTodasPiezasBlandas()
    {
        BonePhysicsController[] piezasBlandas = FindObjectsOfType<BonePhysicsController>();
        foreach (BonePhysicsController pieza in piezasBlandas)
        {
            pieza.ActivarFisicas();
        }
        Debug.Log($"Se han ablandado {piezasBlandas.Length} piezas blandas en la escena.");
    }

    // --- FIN FUNCIONES ---

    // Botón StartGame desde el inventario, redirige al Manager del tiempo
    public void StartGame()
    {
        if (TimerAndShopManager.Instance != null)
        {
            TimerAndShopManager.Instance.StartGame();
        }
        else
        {
            Debug.LogWarning("No se encontró el TimerAndShopManager en la escena");
        }
    }

    // ── GESTIÓN DE AVISOS ──
    private void MostrarAvisoPiezas()
    {
        if (panelAvisoPiezas != null)
        {
            if (rutinaAvisoPiezas != null) StopCoroutine(rutinaAvisoPiezas);
            rutinaAvisoPiezas = StartCoroutine(RutinaMostrarYDesvanecerAviso());
        }
        else
        {
            Debug.LogWarning("Configura el panelAvisoPiezas en el Inspector para ver el mensaje en pantalla");
        }
    }

    private System.Collections.IEnumerator RutinaMostrarYDesvanecerAviso()
    {
        // Mostrar de golpe
        panelAvisoPiezas.gameObject.SetActive(true);
        Color colorFondo = panelAvisoPiezas.color;
        colorFondo.a = 1f;
        panelAvisoPiezas.color = colorFondo;

        // Esperar
        yield return new WaitForSecondsRealtime(tiempoMostrarAviso);

        // Fade out
        float tiempo = 0f;
        while (tiempo < tiempoFadeOut)
        {
            tiempo += Time.unscaledDeltaTime;
            colorFondo.a = Mathf.Lerp(1f, 0f, tiempo / tiempoFadeOut);
            panelAvisoPiezas.color = colorFondo;
            yield return null;
        }

        colorFondo.a = 0f;
        panelAvisoPiezas.color = colorFondo;
        panelAvisoPiezas.gameObject.SetActive(false);
    }
}
