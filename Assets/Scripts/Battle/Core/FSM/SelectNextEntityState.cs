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
        // 获取下一位行动对象
        BattleEntity nextEntity = _controller.GetNextActorByRound();

        // 如果没有可行动对象，则战斗结束
        if (nextEntity == null) {
            _controller.StopBattle();
            yield break;
        }

        // 将“当前行动者”切换为下一为行动对象
        _controller.CurrentEntity = nextEntity;
        _controller.UpdateTimelinePrediction();

        // 等待回合开始停顿
        if (_controller.Config.TurnStartDelay > 0) {
            yield return new WaitForSeconds(_controller.Config.TurnStartDelay);
        }

        // 选择当前行动者并广播当前行动者变化事件
        _controller.TimelineUI.SetActiveEntity(nextEntity);
        EventBus.Publish(new ActiveEntityChangedEvent(nextEntity));

        // 根据当前行动者切换输入模式
        if (nextEntity != null) {
            if (nextEntity.IsPlayer) {
                _controller.SetState(new PlayerInputState(_controller));
            }
            else {
                _controller.SetState(new EnemyAIState(_controller));
            }
        }

        yield break;
    }
}
