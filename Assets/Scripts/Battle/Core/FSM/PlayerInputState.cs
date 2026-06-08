
using System;

/// <summary>
/// 玩家输入状态
/// </summary>
public class PlayerInputState : BattleState {
    private bool _inputReceived;

    public PlayerInputState(BattleController controller) : base(controller) { }

    public override IEnumerator Enter() {
        // 单位移动到行动位置
        yield return MoveCurrentEntityToActionPosition();

        // 请求打开玩家可用指令UI面板
        BattleCommandUI.Instance.RequestInput(_controller.CurrentEntity, OnCommandSelected);

        yield break;
    }

    public override IEnumerator Execute() {
        while (!_inputReceived) {
            yield return null;
        }

        _controller.SetState(NeedsTargetSelection() ?
            new TargetSelectionState(_controller) :
            new PerformActionState(_controller));
        yield break;
    }

    /// <summary>
    /// 将当前单位移动到行动位置
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveCurrentEntityToActionPosition() {
        BattleUnit unit = _controller.CurrentEntity.Unit;
        Vector3 actionPos = _controller.FieldManager.GetActionPos(unit);
        yield return unit.MoveToPosition(actionPos);
    }

    private bool NeedsTargetSelection() {
        return _controller.CurrentCommandRequest.Type == BattleCommandType.Attack ||
            _controller.CurrentCommandRequest.Type == BattleCommandType.Skill;
    }

    #region UI回调函数
    /// <summary>
    /// 玩家选择指令后的回调函数
    /// </summary>
    /// <param name="type">指令类型</param>
    private void OnCommandSelected(BattleCommandType type) {
        switch(type) {
            case BattleCommandType.Attack:
                ConfirmInput(BattleCommandRequest.CreateAttack());
                break;
            case BattleCommandType.Skill:
                ConfirmInput(BattleCommandRequest.CreateAttack());
                break;
            case BattleCommandType.Item:
                ConfirmInput(BattleCommandRequest.CreateAttack());
                break;
            case BattleCommandType.Defend:
                ConfirmInput(BattleCommandRequest.CreateAttack());
                break;
            case BattleCommandType.Escape:
                ConfirmInput(BattleCommandRequest.CreateAttack());
                break;
            default:
                Debug.LogError($"未知的指令类型: {type}");
                break;
        }
    }

    /// <summary>
    /// 确认玩家输入的指令，并将其传递给BattleController
    /// </summary>
    /// <param name="command"></param>
    private void ConfirmInput(BattleCommandRequest command) {
        _controller.CurrentCommandRequest = command;
        _inputReceived = true;

    }
    #endregion
}
