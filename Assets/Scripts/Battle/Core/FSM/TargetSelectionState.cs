
/// <summary>
/// 目标选择状态，负责处理战斗中选择目标的逻辑。
/// <remarks>
/// 该状态主要负责：
/// 1. 根据当前命令收集可选目标列表，并根据玩家输入或AI决策进行目标选择。
/// 2. 处理单体目标时左右切换，群体目标时整体高亮
/// 3. 确认后把选择的目标设置到BattleCommandRequest中
/// 4. 取消则回到玩家输入状态
/// 
/// 可以理解为：
/// “命令确定后选择目标”
/// </remarks>
/// </summary>
public class TargetSelectionState : BattleState {
    public TargetSelectionState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute() {
        _controller.StopBattle();
        yield break;
    }
}
