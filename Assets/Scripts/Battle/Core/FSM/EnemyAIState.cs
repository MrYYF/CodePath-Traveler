using System;

public class EnemyAIState : BattleState {
    public EnemyAIState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute() {
        _controller.CurrentCommandRequest = BuildAICommand();
        yield return new WaitForSeconds(_controller.Config.AIThinkDuration);
        _controller.SetState(new PerformActionState(_controller));
    }

    /// <summary>
    /// 构建AI行动指令
    /// </summary>
    /// <returns></returns>
    private BattleCommandRequest BuildAICommand() {
        BattleEntity actor = _controller.CurrentEntity;
        SkillDataSO selectedSkill = ChooseFirstAvailableSkill(actor);

        // 构建指令
        BattleCommandRequest command =
            selectedSkill == actor.Definition.BasicAttack ?
            BattleCommandRequest.CreateAttack(actor) :
            BattleCommandRequest.CreateSkill(selectedSkill);

        // 选取目标
        AutoTargetSelection(command);
        return command;
    }

    /// <summary>
    /// 根据SP数值选择第一个可用技能
    /// </summary>
    /// <param name="actor">当前行动对象</param>
    /// <returns>技能数据</returns>
    private SkillDataSO ChooseFirstAvailableSkill(BattleEntity actor) {
        if (actor.Definition.InitialSkills == null) {
            return null;
        }

        for (int i = 0; i < actor.Definition.InitialSkills.Count; i++) {
            SkillDataSO skill = actor.Definition.InitialSkills[i];
            if (skill.spCost <= actor.CurrentSP) {
                return skill;
            }
        }

        return actor.Definition.BasicAttack;
    }

    /// <summary>
    /// 自动根据行动指令选择目标
    /// </summary>
    /// <param name="command">行动指令</param>
    private void AutoTargetSelection(BattleCommandRequest command) {
        SkillDataSO skill = command.Skill;

        // 群体目标
        if (skill.targetType != TargetType.SingleAlly && skill.targetType != TargetType.SingleEnemy) {
            command.TargetEntityID = null;
            return;
        }

        // 单体目标
        List<BattleEntity> candidates = BattleTargeting.GetAliveTargetsByType(
            _controller.CurrentEntity,
            skill.targetType,
            _controller.AllEntities
            );

        if (candidates.Count == 0) {
            return;
        }

        bool isSingleAllyHeal = skill.targetType == TargetType.SingleAlly && skill.skillType == SkillType.Heal;
        BattleEntity target = isSingleAllyHeal ?
            GetLowestHPTarget(candidates) :
             candidates[UnityEngine.Random.Range(0, candidates.Count)];
        command.TargetEntityID = target.ID;

    }

    /// <summary>
    /// 获取hp比例最低的实体
    /// </summary>
    /// <param name="candidates">待选实体集合</param>
    /// <returns></returns>
    private BattleEntity GetLowestHPTarget(List<BattleEntity> candidates) {
        BattleEntity bestTarget = candidates[0];
        float bestRatio = bestTarget.CurrentHP / bestTarget.TotalStats.MaxHP;

        for (int i = 0; i < candidates.Count; i++) {
            BattleEntity candidate = candidates[i];
            float ratio = candidate.CurrentHP / candidate.TotalStats.MaxHP;

            if (ratio >= bestRatio) {
                continue;
            }

            bestTarget = candidate;
            bestRatio = ratio;
        }

        return bestTarget;
    }
}
