
/// <summary>
/// 执行行动状态，负责处理战斗中执行行动的逻辑。
/// <remarks>
/// 该状态主要负责：
/// 1. 当前行动者与命令
/// 2. 创建BattleActionContext
/// 3. 把命令传递给BattleCommandExecutor执行 分发到对应handler
/// 4. 执行完毕后刷新时间轴，并进入回合收尾状态
/// 
/// 可以理解为：
/// “真正执行行动的状态”
/// </remarks>
/// </summary>
public class PerformActionState : BattleState {
    private AttackSkillCommandHandler AttackSkillHandler = new();
    private ItemCommandHandler ItemCommandHandler = new();
    private DefendCommandHandler DefendSkillHandler = new();
    private EscapeCommandHandler EscapeCommandHandler = new();

    public PerformActionState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute() {
        BattleEntity entity = _controller.CurrentEntity;
        BattleCommandRequest command = _controller.CurrentCommandRequest;

        switch (command.Type) {
            case BattleCommandType.Attack:
            case BattleCommandType.Skill:
                yield return AttackSkillHandler.Execute(_controller);
                break;
            case BattleCommandType.Item:
                yield return ItemCommandHandler.Execute(_controller);
                break;
            case BattleCommandType.Defend:
                yield return DefendSkillHandler.Execute(_controller);
                break;
            case BattleCommandType.Escape:
                yield return EscapeCommandHandler.Execute(_controller);
                break;
        }

        _controller.UpdateTimelinePrediction();
        _controller.SetState(new TurnEndState(_controller));
        yield break;
    }
}
