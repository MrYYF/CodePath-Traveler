/// <summary>
/// attakc/skill共用处理器
/// 资源扣除、日志、目标解析等前置流程集中管理
/// </summary>
public class AttackSkillCommandHandler : BattleCommandHandleBase {
    private List<BattleEntity> _targets = new();
    private AttackSkillExecutionEngine _executionEngine;


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

        Debug.Log(
            $"[AttackDebug] Name = {Actor.Definition.Name}, " +
            $"Skill = {Command.Skill.skillName}, " +
            $"TargetID = {Command.TargetEntityID}"
            );
        //TODO:显示技能名字

        return true;
    }

    protected override IEnumerator ExecutionPhase() {
        return _executionEngine.Execute(Controller, _targets);
    }


    protected override IEnumerator ResolvePhase() {
        return base.ResolvePhase();
    }
}
