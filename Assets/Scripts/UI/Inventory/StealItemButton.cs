using System;
using TMPro;


/// <summary>
/// 偷窃按钮类，继承自ItemButton，负责显示偷窃物品的信息和处理选中状态
/// </summary>
public class StealItemButton : ItemButton
{
    [Header("Steal Item Button")]
    [SerializeField] private TMP_Text rateText; // 偷取成功率文本

    protected override void SetupButton(InventoryItem inventoryItem, Action<ItemDefinitionSO> onItemClick) {
        base.SetupButton(inventoryItem, onItemClick);

        rateText.text = $"{CurrentItemDefinition.RarityWeight}%";
    }

}
