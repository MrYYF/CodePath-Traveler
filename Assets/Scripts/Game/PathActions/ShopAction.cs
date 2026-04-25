



public class ShopAction : ActionBase
{
    [Header("Shop Action")]
    public List<InventoryItem> itemsBag; // 商店出售的物品列表

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }
}
