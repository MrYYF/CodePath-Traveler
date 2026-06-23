/// <summary>
/// attakc/skill共用处理器
/// 资源扣除、日志、目标解析等前置流程集中管理
/// </summary>
public class AttackSkillCommandHandler : BattleCommandHandleBase {
    private List<BattleEntity> _targets = new();
    private AttackSkillExecutionEngine _executionEngine = new AttackSkillExecutionEngine();

    /// <summary>
    /// 准备阶段
    /// </summary>
    /// <returns></returns>
    protected override bool PreparePhase() {
        // 清空运行时状态
        _targets.Clear();

        // 消耗BP
        if (Command.BPSpend > 0) {
            Actor.SpendBP(Command.BPSpend);
            Actor.MarkBPUsed();
        }

        // 消耗SP
        if (Command.Type == BattleCommandType.Skill) {
            Actor.SpendSP(Command.Skill.spCost);
        }

        // 构建目标
        _targets.AddRange(BattleTargeting.BuildExecutionTargets(Controller));

        //显示技能名字
        EventBus.Publish(new SkillNameDisplayEvent(Actor, Command.Skill.skillName));
        return true;
    }

    /// <summary>
    /// 执行阶段
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator ExecutionPhase() {
        yield return _executionEngine.Execute(Controller, _targets);
    }

    /// <summary>
    /// 等待后摇时间结束
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator ResolvePhase() {
        float recovery = Controller.Config.AttackRecoveryTime;
        yield return new WaitForSeconds(recovery);
    }
}
