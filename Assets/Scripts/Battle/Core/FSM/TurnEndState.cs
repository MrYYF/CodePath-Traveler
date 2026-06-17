/// <summary>
/// 回合结束收尾状态
/// 
/// 行动者归位
/// 清除临时状态
/// 战斗结束判断
/// 切换流程为“选择下一位行动者”
/// </summary>
public class TurnEndState : BattleState
{
    public TurnEndState(BattleController controller) : base(controller) {}

    public override IEnumerator Execute() {
        BattleEntity entity = _controller.CurrentEntity;

        // UI表现恢复

        // 若满足胜利条件，插入停顿进入结算
        yield return new WaitForSeconds(_controller.Config.VictoryResultDelay);

        // 单位回归站位
        Vector3 homePos = _controller.FieldManager.GetHomePos(entity.Unit);
        if (Vector3.Distance(entity.Unit.transform.position, homePos) > 0.1f) {
            yield return _controller.StartCoroutine(entity.Unit.MoveToPosition(homePos, 0.35f));
        }

        //  清除当前命令
        _controller.CurrentCommandRequest = null;

        // 回合结束缓冲时间
        if(_controller.Config.TurnEndDelay > 0) {
            yield return new WaitForSeconds(_controller.Config.TurnEndDelay);
        }

        // 清除当前行动者
        _controller.CurrentEntity = null;

        // 进入“选择下一行动者”状态
        _controller.SetState(new SelectNextEntityState(_controller));
    }

}
