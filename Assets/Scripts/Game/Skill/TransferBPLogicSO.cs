
/// <summary>
/// 将SP转化为BP点
/// </summary>
[CreateAssetMenu(menuName = "Battle/Skill Logic/TransferBPLogicSO")]
public class TransferBPLogicSO : SkillLogicSO {
    public override IEnumerator ExecuteLogic(BattleController controller, BattleEntity actor, BattleCommandRequest command, List<BattleEntity> targets) {
        SkillDataSO skill = command.Skill;
        BoostTierConfig currentTier = skill.GetBoostTier(command.BPSpend);
        int amountToGive = skill.basePower + currentTier.genericValueBonus;
        
        // 播放动画
        actor.Unit.PlayAttackAnimation();
        yield return new WaitForSeconds(controller.Config.AttackWindupTime);

        foreach (var target in targets) {
            int beforeBP = target.CurrentBP;
            int finalAmount = Mathf.Min(amountToGive, 5 - beforeBP);
            if (finalAmount > 0) {
                target.RuntimeData.ModifyBP(finalAmount);
                EventBus.Publish(new EntityStatChangedEvent(target, StatType.CurrentBP, target.CurrentBP, 5));
            }

            controller.SpawnDamagePopup(target, finalAmount, DamageType.Gold);
        }
    }
}
