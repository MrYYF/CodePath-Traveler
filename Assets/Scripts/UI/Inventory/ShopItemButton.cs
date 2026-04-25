


using System;
using TMPro;

public class ShopItemButton : ItemButton
{
    [Header("Shop Item Button")]
    [SerializeField] private TMP_Text priceText; // 显示价格的文本组件

    public void SetupButton(InventoryItem inventoryItem,PanelType panelType ,Action<ItemDefinitionSO> onItemClick) {
        base.SetupButton(inventoryItem, onItemClick);
        priceText.text = panelType switch
        {
            PanelType.Buy => inventoryItem.ItemDefinition.BuyPrice.ToString(),
            PanelType.Sell => inventoryItem.ItemDefinition.SellPrice.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(panelType), panelType, null)
        };

    }

}
