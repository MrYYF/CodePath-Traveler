using System.Linq;

/// <summary>
/// 战斗结果处理器
/// </summary>
public static class BattleOutcomeResolver {
    /// <summary>
    /// 尝试生成战斗结束事件
    /// </summary>
    /// <param name="entities">当前场上实体快照</param>
    /// <param name="endedEvent">输出的战斗结束事件</param>
    /// <returns>可结束返回true，否则返回false</returns>
    public static bool TryGetBattleEndedEvent(List<BattleEntity> entities, out BattleEndedEvent endedEvent) {
        bool hasPlayers = false;
        bool hasEnemies = false;

        foreach (var entity in entities) {
            if (!entity.IsAlive) {
                continue;
            }

            // 场上存活的属于哪方阵营
            if (entity.IsPlayer) {
                hasPlayers = true;
            }
            else {
                hasEnemies = true;
            }

            // 如果双方都有存活实体则继续战斗
            if (hasPlayers && hasEnemies) {
                endedEvent = default;
                return false;
            }
        }

        // 玩家无人存活，判负
        if (!hasPlayers) {
            endedEvent = new BattleEndedEvent(false);
            return true;
        }

        // 胜利的情况，将战利品置入事件中
        int exp = 0;
        int money = 0;
        Dictionary<ItemDefinitionSO, int> dropQuantityMap = new();

        foreach (var entity in entities) {
            if (entity.IsPlayer)
                continue;

            // 经验金币
            EnemyDefinitionSO enemyDef = (EnemyDefinitionSO)entity.Definition;
            exp += enemyDef.ExpReward;
            money += enemyDef.MoneyReward;

            // 掉落物品
            List<InventoryItem> drops = entity.BattleDrops;
            foreach (var dropItem in drops) {
                if (dropItem.Quantity <= 0)
                    continue;

                ItemDefinitionSO itemDefinition = dropItem.ItemDefinition;

                // 根据物品稀有度折算成掉率
                float chance01 = Mathf.Clamp01(itemDefinition.RarityWeight / 100f);
                if (Random.value > chance01) {
                    continue;
                }
                int addQuantity = dropItem.Quantity;

                // 同一个物品只累加数量，不重复生成条目
                if (dropQuantityMap.TryGetValue(itemDefinition, out int quantity)) {
                    dropQuantityMap[itemDefinition] = quantity + addQuantity;
                }
                else {
                    dropQuantityMap.Add(itemDefinition, addQuantity);
                }
            }
        }

        List<BattleDropReward> result = dropQuantityMap
            .Select(pair => new BattleDropReward(pair.Key, pair.Value))
            .ToList();
        endedEvent = new BattleEndedEvent(true, exp, money, result);
        return true;
    }
}
