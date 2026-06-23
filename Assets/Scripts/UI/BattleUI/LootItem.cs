using TMPro;
using UnityEngine.UI;

/// <summary>
/// 结算面板中物品掉落
/// </summary>
public class LootItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemAmount;

    public void SetLootItem(InventoryItem inventoryItem) {
        itemName.text = inventoryItem.ItemDefinition.ItemName;
        itemAmount.text = $"x{inventoryItem.Quantity}";
        itemIcon.sprite = InventoryManager.Instance.IconSet.GetIconForItem(inventoryItem.ItemDefinition.itemIconKey);
    }
}
