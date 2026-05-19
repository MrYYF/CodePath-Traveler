using System;
using static AllyDefinitionSO;

[Serializable]
public class CharacterRuntimeData {
    public CharacterDefinitionSO Definition;

    public int Level;
    public int CurrentHP;
    public int CurrentSP;
    public int CurrentBP;
    public int CurrentExp;
    public string DisplayName => Definition.Name;
    public StatBlock EquipmentStats;

    public bool hasAppliedInitialEquipment = false;

    // TODO: 可能需要改成Dictionary<EquipSlot, EquipmentItemSO>以方便查询和管理
    public List<EquippedItemEntry> EquippedItems = new();

    [Serializable]
    public class EquippedItemEntry {
        public EquipSlot equipSlot;
        public EquipmentItemSO equiptmentItem;
    }

    public CharacterRuntimeData(CharacterDefinitionSO definition) {
        Definition = definition;
        EquipmentStats = StatBlock.Zero;
        Level = definition.BaseLevel;

        var stats = GetBaseStats();
        CurrentHP = stats.MaxHP;
        CurrentSP = stats.MaxSP;
        CurrentBP = 0;

        ApplyInitialEquipment();
    }

    public StatBlock GetBaseStats() {
        if (Definition is AllyDefinitionSO allyDefinition)
            return allyDefinition.GetStatForLevel(Level);

        if (Definition is EnemyDefinitionSO enemyDefinition)
            return enemyDefinition.BaseStats;

        return Definition != null ? Definition.BaseStats : StatBlock.Zero;
    }

    public StatBlock GetTotalStats() => GetBaseStats() + EquipmentStats;


    #region 数据变化接口
    public void HealFull() {
        var stats = GetTotalStats();
        CurrentHP = stats.MaxHP;
        CurrentSP = stats.MaxSP;
    }

    public void ModifyHP(int amount) {
        var stats = GetTotalStats();
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, stats.MaxHP);
    }

    public void ModifySP(int amount) {
        var stats = GetTotalStats();
        CurrentSP = Mathf.Clamp(CurrentSP + amount, 0, stats.MaxSP);
    }

    public void ResetBattleBP() {
        CurrentBP = 0;
    }

    #endregion

    #region 装备系统
    /// <summary>
    /// 应用初始装备的属性加成到角色数据中，并将初始装备添加到角色的物品栏中
    /// </summary>
    public void ApplyInitialEquipment() {
        if (hasAppliedInitialEquipment)
            return;

        AllyDefinitionSO allyDef = Definition as AllyDefinitionSO;

        if (allyDef == null || allyDef.InitialEquipment == null || allyDef.InitialEquipment.Count == 0) {
            hasAppliedInitialEquipment = true; // 即使没有初始装备，也标记为已应用，避免重复检查
            return;
        }

        if (InventoryManager.Instance == null)
            return;

        for (int i = 0; i < allyDef.InitialEquipment.Count; i++) {
            InitialEquipmentEntry entry = allyDef.InitialEquipment[i];
            EquipmentItemSO item = entry.equiptmentItem;

            if (item == null)
                continue;

            //装备物品添加属性
            SetEquippedItem(entry.equipSlot, item);
            InventoryManager.Instance.AddItem(item, 1);
        }

        hasAppliedInitialEquipment = true;
    }

    /// <summary>
    /// 装备或更换指定装备位上的装备物品，如果item参数为null，则表示卸下该装备位上的装备
    /// </summary>
    /// <param name="slot">指定的装备位</param>
    /// <param name="item">要装备的物品，如果为null则表示卸下装备</param>
    public void SetEquippedItem(EquipSlot slot, EquipmentItemSO item) {
        var entry = EquippedItems.Find(e => e.equipSlot == slot);

        if (entry != null) {
            if (item != null) {
                entry.equiptmentItem = item;
            }
            else {
                EquippedItems.Remove(entry);
            }
        }
        else {
            EquippedItems.Add(new EquippedItemEntry { equipSlot = slot, equiptmentItem = item });
        }
        RebuildEquipmentStats();
    }

    /// <summary>
    /// 获取当前角色在指定装备位上装备的装备物品，如果该装备位没有装备物品，则返回null
    /// </summary>
    /// <param name="slot">指定的装备位</param>
    /// <returns>装备物品，如果没有装备则返回null</returns>
    public EquipmentItemSO GetEquippedItem(EquipSlot slot) {
        var entry = EquippedItems.Find(e => e != null && e.equipSlot == slot);
        return entry?.equiptmentItem;
    }

    /// <summary>
    /// 获取当前角色装备了多少件指定的装备物品（可能是同一件装备的多个副本）
    /// </summary>
    /// <param name="item">指定的装备物品</param>
    /// <returns>装备数量</returns>
    public int GetEquippedItemCount(ItemDefinitionSO item) {
        if (item == null) return 0;

        int count = 0;
        foreach (var entry in EquippedItems) {
            if (entry != null && entry.equiptmentItem == item) {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 刷新角色的装备属性加成，遍历当前角色装备的所有装备物品，将它们的属性加成累加到角色的总装备属性中，并更新角色的当前HP和SP以确保它们不超过新的最大值
    /// </summary>
    public void RebuildEquipmentStats() {
        var mergedStats = StatBlock.Zero;
        for (int i = EquippedItems.Count - 1; i >= 0; i--) {
            var entry = EquippedItems[i];

            if (entry == null || entry.equiptmentItem == null) {
                EquippedItems.RemoveAt(i);
                continue;
            }

            mergedStats += entry.equiptmentItem.statBouns;
        }

        EquipmentStats = mergedStats;

        var totalStats = GetTotalStats();
        CurrentHP = Mathf.Clamp(CurrentHP, 0, totalStats.MaxHP);
        CurrentSP = Mathf.Clamp(CurrentSP, 0, totalStats.MaxSP);
    }

    #endregion

    #region 工具静态方法
    /// <summary>
    /// 根据属性数据评估角色的战力，返回一个数值表示角色的综合实力，数值越高表示角色越强大。
    /// </summary>
    /// <param name="stats">角色的总属性</param>
    /// <returns>战力评估值</returns>
    public static int EvaluatePowerFromStats(StatBlock stats) {
        // 简单的战力评估公式，可以根据实际需求调整权重和计算方式
        float power =
            stats.MaxHP * 1.2f +
            stats.MaxSP * 1.2f +
            stats.PAtk * 1.5f +
            stats.PDef * 1.5f +
            stats.MAtk * 1.5f +
            stats.MDef * 1.5f +
            stats.Accuracy * 0.8f +
            stats.Evasion * 0.8f +
            stats.Speed * 0.8f;


        return Mathf.Max(1, Mathf.RoundToInt(power));
    }

    #endregion
}
