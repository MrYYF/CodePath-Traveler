



public class ShopAction : ActionBase
{
    [Header("Shop Action")]
    public List<InventoryItem> itemsBag; // 商店出售的物品列表

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }

    public bool TryExcuteTransaction(PanelType panelType,ItemDefinitionSO itemDefinition) {
        InventoryManager inventoryManager = InventoryManager.Instance;
        int playerCurrency = inventoryManager.Currency;
        switch (panelType) {
            case PanelType.Buy:
                if (!inventoryManager.TrySpendCurrency(itemDefinition.BuyPrice)) {
                    return false; // 货币不足，无法购买
                }
                inventoryManager.AddItem(itemDefinition, 1);
                break;
            case PanelType.Sell:
                int playerItemQuantity = inventoryManager.GetItemQuantity(itemDefinition);
                if (playerItemQuantity <= 0) {
                    return false; // 没有该物品，无法出售
                }
                inventoryManager.RemoveItem(itemDefinition, 1);
                inventoryManager.AddCurrency(itemDefinition.SellPrice);
                break;
            default:
                return false; // 无效的交易类型
        }
        return true; // 交易成功
    }
}
