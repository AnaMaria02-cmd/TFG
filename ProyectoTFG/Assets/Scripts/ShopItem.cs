using UnityEngine;

public class ShopItem : MonoBehaviour
{
    [Header("Datos de la Pieza")]
    public string itemName = "Nueva Pieza";
    public int cost = 10;
    
    [Tooltip("El prefab 3D real de la pieza que el jugador va a comprar")]
    public GameObject piecePrefab;

    // Vincular este método al OnClick() de este mismo Button en el Inspector
    public void OnBuyClicked()
    {
        // 1. Verificamos si tenemos dinero suficiente en el Manager de la Tienda
        if (TimerAndShopManager.Instance != null && TimerAndShopManager.Instance.TrySpendMoney(cost))
        {
            Debug.Log($"¡Compra exitosa! Has comprado: {itemName}");
            
            // 2. Si se pagó con éxito, lo añadimos al Inventario
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddPieceToInventory(piecePrefab, itemName);
            }
            else
            {
                Debug.LogWarning("Falta el InventoryManager en la escena.");
            }
        }
        else
        {
            Debug.LogWarning($"No tienes dinero suficiente para {itemName} (Cuesta {cost}).");
        }
    }
}
