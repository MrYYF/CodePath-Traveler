


public static class EquipmentService
{
    /// <summary>
    /// 构建对于目标装备槽位在库存中的可选装备列表
    /// </summary>
    /// <param name="inventory">库存管理类单例</param>
    /// <param name="targetSlot">目标槽位</param>
    /// <returns></returns>
    public static List<EquipmentItemSO> BuildCandidates(InventoryManager inventory,EquipSlot targetSlot) {
        List<EquipmentItemSO> result = new List<EquipmentItemSO>();

        for (int i = 0; i < inventory.CurrentInventory.Count; i++) {
            InventoryItem item = inventory.CurrentInventory[i];

            // 非空
            if (item == null || item.Quantity <= 0)
                continue;

            // 物品类型为装备
            if (item.ItemDefinition is not EquipmentItemSO equipmentItem)
                continue;

            // 物品槽位对应
            if (!IsSlotCompatible(equipmentItem, targetSlot))
                continue;

            // 添加到展示列表
            result.Add(equipmentItem);
        }

        return result;
    }

    /// <summary>
    /// 用于判断物品与装备槽位是否匹配
    /// </summary>
    /// <param name="item">装备物品信息</param>
    /// <param name="slot">槽位</param>
    /// <returns>是否匹配</returns>
    private static bool IsSlotCompatible(EquipmentItemSO item, EquipSlot slot) {
        if (item == null) return false;

        return item.equipmentCategory switch {
            EquipmentCategory.Weapon => slot == (EquipSlot)((int)item.weaponType - 1),
            EquipmentCategory.Shield => slot == EquipSlot.Shield,
            EquipmentCategory.Head => slot == EquipSlot.Head,
            EquipmentCategory.Body => slot == EquipSlot.Body,
            EquipmentCategory.Accessory => slot == EquipSlot.Accessory1 || slot == EquipSlot.Accessory2,
            _ => false,
        };
    }

    /// <summary>
    /// 获取物品可用数量（总数量 - 已装备数量），如果是非装备物品，直接返回全部数量
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="party"></param>
    /// <param name="itemDefinition"></param>
    /// <returns></returns>
    public static int GetAvailableItemCount(InventoryManager inventory,PartyManager party, ItemDefinitionSO item) {
        if (item == null) 
            return 0;
        int totalQuantity = inventory.GetItemQuantity(item);
        if (totalQuantity <= 0)
            return 0;

        for (int i = 0; i < party.PartyMembers.Count; i++) {
            totalQuantity -= party.PartyMembers[i].GetEquippedItemCount(item);
        }

        return totalQuantity;
    }
}
