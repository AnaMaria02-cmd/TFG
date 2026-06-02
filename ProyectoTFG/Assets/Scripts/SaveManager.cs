using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SaveManager
{
    private const string KEY_HAS_SAVE = "HasSaveGame";
    private const string KEY_COINS = "Save_Coins";
    private const string KEY_MULTIPLIER = "Save_Multiplier";
    private const string KEY_QTY_LARGA = "Save_QtyLarga";
    private const string KEY_QTY_BLANDA = "Save_QtyBlanda";
    private const string KEY_QTY_IMAN = "Save_QtyIman";
    private const string KEY_COST_LARGA = "Save_CostLarga";
    private const string KEY_COST_BLANDA = "Save_CostBlanda";
    private const string KEY_COST_IMAN = "Save_CostIman";
    private const string KEY_COST_DOBLE = "Save_CostDoble";
    private const string KEY_COST_MATERIAL = "Save_CostMaterial";
    private const string KEY_COST_VICTORIA = "Save_CostVictoria";
    private const string KEY_TIME_REMAINING = "Save_TimeRemaining";
    private const string KEY_MATERIAL_MODIFIED = "Save_MaterialModified";
    private const string KEY_PLAYER_X = "Save_PlayerX";
    private const string KEY_PLAYER_Y = "Save_PlayerY";
    private const string KEY_PLAYER_Z = "Save_PlayerZ";
    private const string KEY_PLAYER_RX = "Save_PlayerRX";
    private const string KEY_PLAYER_RY = "Save_PlayerRY";
    private const string KEY_PLAYER_RZ = "Save_PlayerRZ";
    private const string KEY_PLAYER_RW = "Save_PlayerRW";

    // Struct para manejar la conexión de sockets en el hilo de carga
    private struct PendingAttach
    {
        public DropAndDrag piece;
        public Vector3 socketPos;
    }

    public static bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
    }

    // Filtra si un GameObject con DropAndDrag es una pieza real individual o un sub-objeto de un prefab de pieza
    private static bool IsRealPiece(DropAndDrag piece)
    {
        if (piece == null) return false;

        // Si no tiene padre con DropAndDrag en su jerarquía, es una pieza raíz real
        DropAndDrag parentPiece = piece.transform.parent != null ? piece.transform.parent.GetComponentInParent<DropAndDrag>() : null;
        if (parentPiece == null)
        {
            return true;
        }

        // Si tiene un padre DropAndDrag, para ser una pieza acoplada real distinta,
        // debe estar conectada a través de un objeto con el componente Socket.
        if (piece.transform.parent != null && piece.transform.parent.GetComponent<Socket>() != null)
        {
            return true;
        }

        // Si tiene un padre DropAndDrag pero no está conectado a un Socket,
        // significa que es un sub-componente interno del propio prefab de la pieza (ej. un Cube hijo del Imán)
        return false;
    }

    public static void SaveGame()
    {
        Debug.Log("Iniciando guardado de partida...");

        // Guardamos que existe una partida
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);

        // 1. Obtener CoinCounter (Priorizando la referencia asignada en el inspector de TimerAndShopManager)
        CoinCounter coins = null;
        if (TimerAndShopManager.Instance != null)
        {
            coins = TimerAndShopManager.Instance.coinCounter;
        }
        if (coins == null)
        {
            coins = Object.FindObjectOfType<CoinCounter>();
        }

        if (coins != null)
        {
            int currentCoins = coins.GetCurrentCoins();
            PlayerPrefs.SetInt(KEY_COINS, currentCoins);
            PlayerPrefs.SetInt(KEY_MULTIPLIER, coins.coinMultiplier);
            Debug.Log($"Monedas guardadas: {currentCoins}, Multiplicador: {coins.coinMultiplier} (Leído de {coins.gameObject.name})");
        }
        else
        {
            Debug.LogError("Error: ¡No se encontró CoinCounter al guardar!");
        }

        // 2. Guardar Inventario
        InventoryManager inv = InventoryManager.Instance;
        if (inv != null)
        {
            PlayerPrefs.SetInt(KEY_QTY_LARGA, inv.GetCantidadLarga());
            PlayerPrefs.SetInt(KEY_QTY_BLANDA, inv.GetCantidadBlanda());
            PlayerPrefs.SetInt(KEY_QTY_IMAN, inv.GetCantidadIman());
            Debug.Log($"Inventario guardado: Larga({inv.GetCantidadLarga()}), Blanda({inv.GetCantidadBlanda()}), Iman({inv.GetCantidadIman()})");
        }
        else
        {
            Debug.LogError("Error: ¡No se encontró InventoryManager al guardar!");
        }

        // 3. Guardar Datos de la Tienda y el Tiempo
        TimerAndShopManager shop = TimerAndShopManager.Instance;
        if (shop != null)
        {
            PlayerPrefs.SetInt(KEY_COST_LARGA, shop.costeLarga);
            PlayerPrefs.SetInt(KEY_COST_BLANDA, shop.costeBlanda);
            PlayerPrefs.SetInt(KEY_COST_IMAN, shop.costeIman);
            PlayerPrefs.SetInt(KEY_COST_DOBLE, shop.costeDobleDinero);
            PlayerPrefs.SetInt(KEY_COST_MATERIAL, shop.costeModificarMaterial);
            PlayerPrefs.SetInt(KEY_COST_VICTORIA, shop.costeVictoria);
            PlayerPrefs.SetFloat(KEY_TIME_REMAINING, shop.timeRemaining);

            // Guardar si el material ya fue modificado
            bool matModificado = false;
            if (shop.materialAModificar != null)
            {
                if (shop.materialAModificar.color == shop.nuevoColorMaterial)
                {
                    matModificado = true;
                }
            }
            PlayerPrefs.SetInt(KEY_MATERIAL_MODIFIED, matModificado ? 1 : 0);

            // 4. Guardar Posición y Rotación del Jugador
            if (shop.jugadorTransform != null)
            {
                Vector3 pos = shop.jugadorTransform.position;
                PlayerPrefs.SetFloat(KEY_PLAYER_X, pos.x);
                PlayerPrefs.SetFloat(KEY_PLAYER_Y, pos.y);
                PlayerPrefs.SetFloat(KEY_PLAYER_Z, pos.z);

                Quaternion rot = shop.jugadorTransform.rotation;
                PlayerPrefs.SetFloat(KEY_PLAYER_RX, rot.x);
                PlayerPrefs.SetFloat(KEY_PLAYER_RY, rot.y);
                PlayerPrefs.SetFloat(KEY_PLAYER_RZ, rot.z);
                PlayerPrefs.SetFloat(KEY_PLAYER_RW, rot.w);
                Debug.Log($"Jugador guardado en Pos: {pos}, Rot: {rot}");
            }
        }
        else
        {
            Debug.LogError("Error: ¡No se encontró TimerAndShopManager al guardar!");
        }

        // 5. Guardar Basura Activa (Latas/No Conductores no recogidos)
        string coinTag = "Coin";
        string nonConductorTag = "NoConductor";
        if (coins != null)
        {
            coinTag = coins.coinTag;
            nonConductorTag = coins.nonConductorTag;
        }

        List<Vector3> activeTrashPositions = new List<Vector3>();
        
        try
        {
            foreach (GameObject coin in GameObject.FindGameObjectsWithTag(coinTag))
            {
                activeTrashPositions.Add(coin.transform.position);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"El tag '{coinTag}' no está registrado en el Unity Tag Manager. Se ignorarán estas monedas al guardar. Detalles: {e.Message}");
        }

        try
        {
            foreach (GameObject nc in GameObject.FindGameObjectsWithTag(nonConductorTag))
            {
                activeTrashPositions.Add(nc.transform.position);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"El tag '{nonConductorTag}' no está registrado en el Unity Tag Manager. Se ignorarán estos no-conductores al guardar. Detalles: {e.Message}");
        }

        PlayerPrefs.SetInt("Save_TrashCount", activeTrashPositions.Count);
        for (int i = 0; i < activeTrashPositions.Count; i++)
        {
            PlayerPrefs.SetFloat($"Save_TrashX_{i}", activeTrashPositions[i].x);
            PlayerPrefs.SetFloat($"Save_TrashY_{i}", activeTrashPositions[i].y);
            PlayerPrefs.SetFloat($"Save_TrashZ_{i}", activeTrashPositions[i].z);
        }
        Debug.Log($"Basura guardada: {activeTrashPositions.Count} objetos activos.");

        // 6. Guardar Piezas Colocadas en el Escenario y sus conexiones
        DropAndDrag[] allPiecesRaw = Object.FindObjectsOfType<DropAndDrag>();
        List<DropAndDrag> allPieces = new List<DropAndDrag>();
        foreach (var p in allPiecesRaw)
        {
            if (IsRealPiece(p))
            {
                allPieces.Add(p);
            }
        }

        PlayerPrefs.SetInt("Save_PiecesCount", allPieces.Count);
        for (int i = 0; i < allPieces.Count; i++)
        {
            DropAndDrag piece = allPieces[i];

            // Identificar tipo de pieza basándose en el nombre de su GameObject (muy seguro y libre de falsos positivos en jerarquías)
            string type = "Larga";
            string objName = piece.gameObject.name.ToLower();
            if (objName.Contains("magnet") || objName.Contains("iman"))
            {
                type = "Iman";
            }
            else if (objName.Contains("soft") || objName.Contains("blanda") || objName.Contains("bone"))
            {
                type = "Blanda";
            }

            PlayerPrefs.SetString($"Save_PieceType_{i}", type);
            Debug.Log($"Guardando pieza index {i}: Tipo={type}, NombreGameObject={piece.gameObject.name}");

            // Posición y rotación en el mundo
            Vector3 pos = piece.transform.position;
            Quaternion rot = piece.transform.rotation;
            PlayerPrefs.SetFloat($"Save_PiecePX_{i}", pos.x);
            PlayerPrefs.SetFloat($"Save_PiecePY_{i}", pos.y);
            PlayerPrefs.SetFloat($"Save_PiecePZ_{i}", pos.z);
            PlayerPrefs.SetFloat($"Save_PieceRX_{i}", rot.x);
            PlayerPrefs.SetFloat($"Save_PieceRY_{i}", rot.y);
            PlayerPrefs.SetFloat($"Save_PieceRZ_{i}", rot.z);
            PlayerPrefs.SetFloat($"Save_PieceRW_{i}", rot.w);

            // Guardar conexión a sockets (si tiene un padre Socket)
            bool attached = piece.transform.parent != null && piece.transform.parent.GetComponent<Socket>() != null;
            PlayerPrefs.SetInt($"Save_PieceAttached_{i}", attached ? 1 : 0);
            if (attached)
            {
                Transform socketTrans = piece.transform.parent;
                Transform socketOwner = socketTrans.parent;

                PlayerPrefs.SetString($"Save_PieceSocketName_{i}", socketTrans.name);
                
                // Determinar el dueño del socket (si es el jugador o es otra pieza)
                if (socketOwner != null && shop != null && (socketOwner == shop.jugadorTransform || socketOwner.IsChildOf(shop.jugadorTransform)))
                {
                    PlayerPrefs.SetString($"Save_PieceParentType_{i}", "Player");
                }
                else
                {
                    DropAndDrag parentPiece = socketOwner != null ? socketOwner.GetComponentInParent<DropAndDrag>() : null;
                    int parentIndex = -1;
                    if (parentPiece != null)
                    {
                        parentIndex = allPieces.IndexOf(parentPiece);
                    }

                    if (parentIndex != -1)
                    {
                        PlayerPrefs.SetString($"Save_PieceParentType_{i}", "Piece");
                        PlayerPrefs.SetInt($"Save_PieceParentIndex_{i}", parentIndex);
                    }
                    else
                    {
                        PlayerPrefs.SetString($"Save_PieceParentType_{i}", "Unknown");
                    }
                }
            }
        }
        Debug.Log($"Piezas de robot guardadas: {allPieces.Count} piezas reales.");

        PlayerPrefs.Save();
        Debug.Log("¡Partida Guardada Exitosamente!");
    }

    public static void LoadGame()
    {
        if (!HasSavedGame())
        {
            Debug.LogWarning("No hay ninguna partida guardada.");
            return;
        }

        Debug.Log("Iniciando carga de partida...");

        // 1. Obtener CoinCounter (Priorizando la referencia asignada en el inspector de TimerAndShopManager)
        CoinCounter coins = null;
        if (TimerAndShopManager.Instance != null)
        {
            coins = TimerAndShopManager.Instance.coinCounter;
        }
        if (coins == null)
        {
            coins = Object.FindObjectOfType<CoinCounter>();
        }

        if (coins != null)
        {
            int loadedCoins = PlayerPrefs.GetInt(KEY_COINS, 0);
            int loadedMultiplier = PlayerPrefs.GetInt(KEY_MULTIPLIER, 1);
            coins.LoadCoinsData(loadedCoins, loadedMultiplier);
            Debug.Log($"Monedas cargadas: {loadedCoins}, Multiplicador: {loadedMultiplier} (Cargado en {coins.gameObject.name})");
        }
        else
        {
            Debug.LogError("Error: ¡No se encontró CoinCounter al cargar!");
        }

        // 2. Cargar Inventario
        InventoryManager inv = InventoryManager.Instance;
        if (inv != null)
        {
            int larga = PlayerPrefs.GetInt(KEY_QTY_LARGA, 0);
            int blanda = PlayerPrefs.GetInt(KEY_QTY_BLANDA, 0);
            int iman = PlayerPrefs.GetInt(KEY_QTY_IMAN, 0);
            inv.LoadInventoryData(larga, blanda, iman);
            Debug.Log($"Inventario cargado: Larga({larga}), Blanda({blanda}), Iman({iman})");
        }
        else
        {
            Debug.LogError("Error: ¡No se encontró InventoryManager al cargar!");
        }

        // 3. Cargar Tienda y Tiempo
        TimerAndShopManager shop = TimerAndShopManager.Instance;
        if (shop != null)
        {
            int cLarga = PlayerPrefs.GetInt(KEY_COST_LARGA, 5);
            int cBlanda = PlayerPrefs.GetInt(KEY_COST_BLANDA, 8);
            int cIman = PlayerPrefs.GetInt(KEY_COST_IMAN, 20);
            int cDoble = PlayerPrefs.GetInt(KEY_COST_DOBLE, 30);
            int cMaterial = PlayerPrefs.GetInt(KEY_COST_MATERIAL, 50);
            int cVictoria = PlayerPrefs.GetInt(KEY_COST_VICTORIA, 100);
            float timeRem = PlayerPrefs.GetFloat(KEY_TIME_REMAINING, 60f);
            bool matMod = PlayerPrefs.GetInt(KEY_MATERIAL_MODIFIED, 0) == 1;

            shop.LoadShopData(cLarga, cBlanda, cIman, cDoble, cMaterial, cVictoria, timeRem, matMod);

            // 4. Cargar Posición y Rotación del Jugador
            if (shop.jugadorTransform != null && PlayerPrefs.HasKey(KEY_PLAYER_X))
            {
                float x = PlayerPrefs.GetFloat(KEY_PLAYER_X);
                float y = PlayerPrefs.GetFloat(KEY_PLAYER_Y);
                float z = PlayerPrefs.GetFloat(KEY_PLAYER_Z);

                CharacterController cc = shop.jugadorTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                shop.jugadorTransform.position = new Vector3(x, y, z);
                
                if (PlayerPrefs.HasKey(KEY_PLAYER_RX))
                {
                    float rx = PlayerPrefs.GetFloat(KEY_PLAYER_RX);
                    float ry = PlayerPrefs.GetFloat(KEY_PLAYER_RY);
                    float rz = PlayerPrefs.GetFloat(KEY_PLAYER_RZ);
                    float rw = PlayerPrefs.GetFloat(KEY_PLAYER_RW);
                    shop.jugadorTransform.rotation = new Quaternion(rx, ry, rz, rw);
                }

                if (cc != null) cc.enabled = true;
                Debug.Log($"Jugador posicionado en Pos: {shop.jugadorTransform.position}, Rot: {shop.jugadorTransform.rotation}");
            }
        }
        else
        {
            Debug.LogError("Error: ¡No se encontró TimerAndShopManager al cargar!");
        }

        // 5. Filtrar la Basura Activa (Latas/No Conductores recogidos en el guardado)
        int trashCount = PlayerPrefs.GetInt("Save_TrashCount", -1);
        if (trashCount >= 0)
        {
            List<Vector3> savedTrashPositions = new List<Vector3>();
            for (int i = 0; i < trashCount; i++)
            {
                float tx = PlayerPrefs.GetFloat($"Save_TrashX_{i}");
                float ty = PlayerPrefs.GetFloat($"Save_TrashY_{i}");
                float tz = PlayerPrefs.GetFloat($"Save_TrashZ_{i}");
                savedTrashPositions.Add(new Vector3(tx, ty, tz));
            }

            string cTag = "Coin";
            string ncTag = "NoConductor";
            if (coins != null)
            {
                cTag = coins.coinTag;
                ncTag = coins.nonConductorTag;
            }

            List<GameObject> currentTrash = new List<GameObject>();
            
            try
            {
                currentTrash.AddRange(GameObject.FindGameObjectsWithTag(cTag));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"El tag '{cTag}' no está registrado en el Unity Tag Manager. Detalles: {e.Message}");
            }

            try
            {
                currentTrash.AddRange(GameObject.FindGameObjectsWithTag(ncTag));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"El tag '{ncTag}' no está registrado en el Unity Tag Manager. Detalles: {e.Message}");
            }

            int destroyedTrashCount = 0;
            foreach (GameObject trash in currentTrash)
            {
                bool matchesSaved = false;
                for (int i = 0; i < savedTrashPositions.Count; i++)
                {
                    if (Vector3.Distance(trash.transform.position, savedTrashPositions[i]) < 0.2f)
                    {
                        matchesSaved = true;
                        savedTrashPositions.RemoveAt(i); // Quitar para evitar doble match
                        break;
                    }
                }

                // Si no estaba en el guardado, es que ya se recogió. Lo destruimos.
                if (!matchesSaved)
                {
                    Object.Destroy(trash);
                    destroyedTrashCount++;
                }
            }
            Debug.Log($"Limpieza de basura: {destroyedTrashCount} objetos eliminados porque ya habían sido recogidos.");
        }

        // 6. Cargar y Reconstruir Piezas de Robot en la escena
        // Destruimos primero las piezas actuales en la escena para no duplicarlas
        DropAndDrag[] currentPiecesRaw = Object.FindObjectsOfType<DropAndDrag>();
        foreach (var p in currentPiecesRaw)
        {
            if (!IsRealPiece(p)) continue; // Solo procesamos piezas raíces para evitar destruir hijos de forma redundante

            if (p.transform.parent != null)
            {
                Socket s = p.transform.parent.GetComponent<Socket>();
                if (s != null) s.isOccupied = false;
            }
            p.gameObject.SetActive(false); // Desactivar para que sus sockets se oculten y no sean elegibles
            Object.Destroy(p.gameObject);
        }

        int piecesCount = PlayerPrefs.GetInt("Save_PiecesCount", 0);
        DropAndDrag[] spawnedPieces = new DropAndDrag[piecesCount];

        for (int i = 0; i < piecesCount; i++)
        {
            string type = PlayerPrefs.GetString($"Save_PieceType_{i}", "Larga");
            float px = PlayerPrefs.GetFloat($"Save_PiecePX_{i}");
            float py = PlayerPrefs.GetFloat($"Save_PiecePY_{i}");
            float pz = PlayerPrefs.GetFloat($"Save_PiecePZ_{i}");
            float rx = PlayerPrefs.GetFloat($"Save_PieceRX_{i}");
            float ry = PlayerPrefs.GetFloat($"Save_PieceRY_{i}");
            float rz = PlayerPrefs.GetFloat($"Save_PieceRZ_{i}");
            float rw = PlayerPrefs.GetFloat($"Save_PieceRW_{i}");

            Vector3 pos = new Vector3(px, py, pz);
            Quaternion rot = new Quaternion(rx, ry, rz, rw);

            GameObject prefab = null;
            if (inv != null)
            {
                if (type == "Larga") prefab = inv.prefabLarga;
                else if (type == "Blanda") prefab = inv.prefabBlanda;
                else if (type == "Iman") prefab = inv.prefabIman;
            }

            if (prefab != null)
            {
                GameObject spawned = Object.Instantiate(prefab, pos, rot);
                DropAndDrag dd = spawned.GetComponent<DropAndDrag>();
                spawnedPieces[i] = dd;
            }
            else
            {
                Debug.LogError($"Error: ¡No se encontró el prefab para la pieza tipo {type}!");
            }
        }

        // Hacemos un pase de reconexión de sockets basado en jerarquía exacta
        int successfullyAttached = 0;
        for (int i = 0; i < piecesCount; i++)
        {
            DropAndDrag dd = spawnedPieces[i];
            if (dd == null) continue;

            bool attached = PlayerPrefs.GetInt($"Save_PieceAttached_{i}", 0) == 1;
            if (attached)
            {
                string parentType = PlayerPrefs.GetString($"Save_PieceParentType_{i}", "Unknown");
                string socketName = PlayerPrefs.GetString($"Save_PieceSocketName_{i}", "");

                Transform ownerTransform = null;
                if (parentType == "Player" && shop != null)
                {
                    ownerTransform = shop.jugadorTransform;
                }
                else if (parentType == "Piece")
                {
                    int parentIndex = PlayerPrefs.GetInt($"Save_PieceParentIndex_{i}", -1);
                    if (parentIndex >= 0 && parentIndex < spawnedPieces.Length && spawnedPieces[parentIndex] != null)
                    {
                        ownerTransform = spawnedPieces[parentIndex].transform;
                    }
                }

                if (ownerTransform != null)
                {
                    // Buscar el socket por nombre en el dueño
                    Socket targetSocket = null;
                    Socket[] candidateSockets = ownerTransform.GetComponentsInChildren<Socket>(true);
                    foreach (Socket s in candidateSockets)
                    {
                        if (s.gameObject.name == socketName && !s.isOccupied)
                        {
                            targetSocket = s;
                            break;
                        }
                    }

                    if (targetSocket != null)
                    {
                        dd.ForceAttachToSocket(targetSocket);
                        successfullyAttached++;
                    }
                    else
                    {
                        Debug.LogWarning($"Aviso: No se encontró un socket libre con nombre {socketName} en el objeto {ownerTransform.name} para la pieza {dd.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Aviso: No se pudo determinar el dueño del socket para la pieza {dd.name}");
                }
            }
        }

        Debug.Log($"Piezas de robot cargadas: {piecesCount} instanciadas, {successfullyAttached} reconectadas a sockets.");
        Debug.Log("¡Carga de Partida Completada con Éxito!");
    }
}
