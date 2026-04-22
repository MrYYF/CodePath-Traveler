


public class StealAction : ActionBase
{
    [Header("Steal Action")]
    public List<InventoryItem> stealableItems; // 可偷窃的物品列表

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }
}
