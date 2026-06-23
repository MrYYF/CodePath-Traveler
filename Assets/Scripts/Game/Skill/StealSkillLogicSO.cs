/// <summary>
/// 偷窃敌方目标
/// </summary>
[CreateAssetMenu(menuName = "Battle/Skill Logic/StealSkillLogicSO")]
public class StealSkillLogicSO : SkillLogicSO {

    public string EmptyPocketMessage = "敌人兜里空空如也";
    public string EmptyFailuerMessage = "偷窃失败";
    public float lowHpBonusMax = 0.2f;
    public float breakBonus = 0.15f;

    public override IEnumerator ExecuteLogic(BattleController controller, BattleEntity actor, BattleCommandRequest command, List<BattleEntity> targets) {
        actor.Unit.PlayAttackAnimation();
        yield return new WaitForSeconds(controller.Config.AttackWindupTime);

        SkillDataSO skill = command.Skill;
        BoostTierConfig boostTierConfig = skill.GetBoostTier(command.BPSpend);

        foreach (var target in targets) {
            // 已经被偷空
            if (target.HasBeenRobbed) {
                EventBus.Publish(new BattleNotificationEvent(EmptyPocketMessage, false));
                yield break;
            }

            // 获取总数量与总权重
            int totalQuantity = 0;
            int totalWeight = 0;
            foreach (var drop in target.BattleDrops) {
                if (drop.Quantity <= 0) {
                    continue;
                }
                totalQuantity += drop.Quantity;
                totalWeight += drop.ItemDefinition.RarityWeight * drop.Quantity;
            }

            // 无物品可以偷取
            if (totalQuantity == 0) {
                target.RefreshRobbedState();
                EventBus.Publish(new BattleNotificationEvent(EmptyPocketMessage));
                yield break;
            }

            // 偷不到物品
            if (totalWeight == 0) {
                target.RefreshRobbedState();
                EventBus.Publish(new BattleNotificationEvent(EmptyFailuerMessage));
                yield break;
            }

            // 计算偷取成功率
            float baseChance01 = totalWeight / (float)(totalQuantity * 100);
            float chance = CalculateStealChance01(baseChance01, target, boostTierConfig);
            if (Random.value > chance) {
                EventBus.Publish(new BattleNotificationEvent(EmptyFailuerMessage));
                yield break;
            }

            // 抽取一件物品
            int roll = Random.Range(0, totalWeight);
            int currentWeight = 0;
            InventoryItem targetItemDrop = null;

            foreach (var drop in target.BattleDrops) {
                if (drop.Quantity <= 0) {
                    continue;
                }

                currentWeight += drop.ItemDefinition.RarityWeight * drop.Quantity;

                if (roll >= currentWeight) {
                    continue;
                }

                targetItemDrop = drop;
                break;
            }

            // 添加到背包
            InventoryManager.Instance.AddItem(targetItemDrop.ItemDefinition, 1);
            targetItemDrop.Quantity--;
            target.RefreshRobbedState();

            EventBus.Publish(new BattleNotificationEvent($"偷窃成功{targetItemDrop.ItemDefinition.ItemName}", true));
        }
    }

    /// <summary>
    /// 计算偷取概率
    /// </summary>
    /// <param name="baseChance01">基础概率</param>
    /// <param name="target">偷窃目标</param>
    /// <param name="tier">boost加成信息</param>
    /// <returns>偷取概率</returns>
    private float CalculateStealChance01(float baseChance01, BattleEntity target, BoostTierConfig tier) {
        // 低血量时偷取概率加成
        float hpBonus = 0;
        StatBlock stats = target.RuntimeData.GetTotalStats();
        if (stats.MaxHP > 0) {
            float hpPercent = Mathf.Clamp01(target.RuntimeData.CurrentHP / stats.MaxHP);
            hpBonus = (1 - hpPercent) * lowHpBonusMax;
        }

        // 破盾加成
        float brokenBonus = target.IsBroken ? breakBonus : 0;

        // boost 加成
        float tierBonus = tier.chanceBonus;

        float finalChance = baseChance01 + hpBonus + tierBonus + brokenBonus;

        return Mathf.Clamp01(finalChance);
    }

}
