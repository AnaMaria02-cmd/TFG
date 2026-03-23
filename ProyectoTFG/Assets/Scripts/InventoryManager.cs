using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Interfaz del Inventario")]
    [Tooltip("El panel Content dentro del Scroll View donde aparecerán los botones de piezas.")]
    public Transform inventoryContentPanel;
    
    [Tooltip("El Prefab de Botón que se creará por cada pieza en el inventario.")]
    public GameObject inventoryButtonPrefab;

    [Header("Generación de Piezas")]
    [Tooltip("El punto 3D en la escena donde la pieza aparecerá cuando la uses.")]
    public Transform spawnPoint;

    // La lista de piezas que has acumulado
    private List<GameObject> ownedPieces = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Este método es llamado por ShopItem.cs cuando compras una pieza con éxito
    public void AddPieceToInventory(GameObject piecePrefab, string pieceName)
    {
        if (piecePrefab == null) return;

        ownedPieces.Add(piecePrefab);

        // Crear visualmente un botón en el Panel de Inventario
        if (inventoryButtonPrefab != null && inventoryContentPanel != null)
        {
            GameObject newButton = Instantiate(inventoryButtonPrefab, inventoryContentPanel);
            
            // Si el botón tiene texto, le ponemos el nombre de la pieza
            TextMeshProUGUI btnText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = pieceName;

            // Configuramos qué ocurre al hacer CLIC en el botón del inventario
            Button btnComponent = newButton.GetComponent<Button>();
            if (btnComponent != null)
            {
                btnComponent.onClick.AddListener(() => SpawnPieceInWorld(piecePrefab, newButton));
            }
        }
    }

    // Este método es llamado automáticamente al hacer clic en un botón del inventario
    private void SpawnPieceInWorld(GameObject piecePrefab, GameObject uiButtonAsociado)
    {
        // Calcular la posición donde aparecerá la pieza
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        
        // Spawnear la pieza real en el juego
        Instantiate(piecePrefab, spawnPos, Quaternion.identity);

        // Opcional: Eliminar la pieza del inventario visual tras usarla (consumirla)
        ownedPieces.Remove(piecePrefab);
        Destroy(uiButtonAsociado);
        
        Debug.Log("¡Pieza instanciada desde el inventario!");
    }
}
