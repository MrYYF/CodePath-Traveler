/// <summary>
/// 选择下一位行动者状态
/// <remarks>
/// 该状态主要负责：
/// 1. 检查战斗是否结束
/// 2. 从CTB时间轴中选择下一位行动者
/// 3. 同步当前行动者的数据到HUD显示
/// 4. 根据当前行动者切换玩家输入或AI输入
/// 
/// 可以理解为：
/// “状态机里的回合分发器”
/// </remarks>
/// </summary>
public class SelectNextEntityState : BattleState {
    public SelectNextEntityState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute() {
        BattleEntity nextEntity = _controller.AllEntities.Find(entity => entity.IsPlayer && entity.IsAlive);
        _controller.CurrentEntity = nextEntity;

        // 根据当前行动者切换输入模式
        if (nextEntity != null) {
            if (nextEntity.IsPlayer) {
                _controller.SetState(new PlayerInputState(_controller));
            }
        }

        yield break;
    }
}
