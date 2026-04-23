


public class StealAction : ActionBase
{
    [Header("Steal Action")]
    public List<InventoryItem> stealableItems; // 可偷窃的物品列表

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }

    public bool TrySteal(ItemDefinitionSO itemDefinition) {
        bool success = Random.value <= Mathf.Clamp01(itemDefinition.RarityWeight / 100f); // 根据物品稀有度决定偷窃成功率

        if (success) {
            InventoryManager.Instance.AddItem(itemDefinition, 1); // 成功偷窃，添加物品到玩家库存
            for (int i = 0; i < stealableItems.Count; i++) {
                if (stealableItems[i].ItemDefinition != itemDefinition)
                    continue;
                stealableItems.RemoveAt(i); // 从可偷窃列表中移除已偷窃的物品
            }
        }

        return success;
    }
}
