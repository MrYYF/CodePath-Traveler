
using System;

/// <summary>
/// 玩家选择命令状态
/// </summary>
public class PlayerInputState : BattleState {
    // 输入接收状态
    private bool _inputReceived;
    // 待消耗BP点数
    private int _pendingBoostSpend;

    public PlayerInputState(BattleController controller) : base(controller) { }

    public override IEnumerator Enter() {
        // 单位移动到行动位置
        yield return MoveCurrentEntityToActionPosition();

        // 重置输入暂存状态
        _inputReceived = false;
        _controller.CurrentCommandRequest = null;

        //重置BP表现
        _pendingBoostSpend = 0;
        _controller.FieldManager.SetBoostVfxLevel(0);

        // 请求打开玩家可用指令UI面板
        BattleCommandUI.Instance.RequestInput(_controller.CurrentEntity, OnCommandSelected, OnSkillSelected, null);

        yield break;
    }

    public override IEnumerator Execute() {
        while (!_inputReceived) {
            // 等候玩家输入期间持续监听BP 消耗
            UpdateBoostInput();
            yield return null;
        }

        // 输入完成根据目标规则决定下一个状态
        MoveToNextStateByTargetRule();
    }

    /// <summary>
    /// 监听Boost按键，更新boost消耗
    /// </summary>
    private void UpdateBoostInput() {
        int maxSpend = Mathf.Min(3, _controller.CurrentEntity.CurrentBP);
        if (maxSpend <= 0) {
            return;
        }
        int delta = InputSystemController.Instance.GetBoostDeltra();
        _pendingBoostSpend = Mathf.Clamp(delta + _pendingBoostSpend, 0, maxSpend);
        _controller.FieldManager.SetBoostVfxLevel(_pendingBoostSpend);
    }

    /// <summary>
    /// 根据命令内容取出下一个状态
    /// </summary>
    private void MoveToNextStateByTargetRule() {
        BattleCommandRequest command = _controller.CurrentCommandRequest;

        _controller.SetState(command.Type switch {
            BattleCommandType.Attack or
            BattleCommandType.Skill or
            BattleCommandType.Item => new TargetSelectionState(_controller),
            _ => new PerformActionState(_controller),
        });

    }

    /// <summary>
    /// 将当前单位移动到行动位置
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveCurrentEntityToActionPosition() {
        BattleUnit unit = _controller.CurrentEntity.Unit;
        Vector3 actionPos = _controller.FieldManager.GetActionPos(unit);
        float distance = Vector3.Distance(unit.transform.position, actionPos);
        if (distance > 0.1f) {
            yield return unit.MoveToPosition(actionPos);
        }
    }

    #region UI回调函数
    /// <summary>
    /// 玩家选择指令后的回调函数
    /// </summary>
    /// <param name="type">指令类型</param>
    private void OnCommandSelected(BattleCommandType type) {
        switch (type) {
            //TODO:BP消耗
            case BattleCommandType.Attack:
                ConfirmInput(BattleCommandRequest.CreateAttack(_controller.CurrentEntity, _pendingBoostSpend));
                break;
            case BattleCommandType.Skill:
                ConfirmInput(BattleCommandRequest.CreateSkill(_controller.CurrentCommandRequest.Skill, _pendingBoostSpend));
                break;
            case BattleCommandType.Item:
                ConfirmInput(BattleCommandRequest.CreateItem(_controller.CurrentCommandRequest.ItemDefinition));
                break;
            case BattleCommandType.Defend:
                ConfirmInput(BattleCommandRequest.CreateDefend());
                break;
            case BattleCommandType.Escape:
                ConfirmInput(BattleCommandRequest.CreateEscape());
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

    /// <summary>
    /// 点击技能命令后的回调
    /// </summary>
    /// <param name="skill"></param>
    private void OnSkillSelected(SkillDataSO skill) {
        ConfirmInput(BattleCommandRequest.CreateSkill(skill, _pendingBoostSpend));
    }
    #endregion
}
