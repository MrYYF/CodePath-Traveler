
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
    // 目标类型
    private TargetType _targetType = TargetType.SingleEnemy;
    // 可用的目标选择集合
    private List<BattleEntity> _targets;
    // 当前选择的序列值
    private int _currentIndex;

    private bool _ignoreConfirmThisFrame;
    private float _navigateCooldown;
    private const float InputCooldownTime = 0.15f;

    public TargetSelectionState(BattleController controller) : base(controller) { }

    public override IEnumerator Enter() {
        // 根据当前命令解析目标类型
        if (_controller.CurrentCommandRequest.Skill != null) {
            _targetType = _controller.CurrentCommandRequest.Skill.targetType;
        }
        else if (_controller.CurrentCommandRequest.Type == BattleCommandType.Item) {
            _targetType = TargetType.SingleAlly;
        }
        else {
            _targetType = TargetType.SingleEnemy;
        }
        // 按照目标类型收集所有可选目标
        _targets = BattleTargeting.GetAliveTargetsByType(
            _controller.CurrentEntity,
            _targetType,
            _controller.AllEntities
            );

        // 重置这一轮目标选择的运行时状态
        _currentIndex = 0;
        _ignoreConfirmThisFrame = true;
        _navigateCooldown = InputCooldownTime;


        // 如果没有可选目标就跳到执行层
        if (_targets.Count == 0) {
            _controller.SetState(new PerformActionState(_controller));
            yield break;
        }

        // 群体目标直接全部选择
        if (_targetType == TargetType.AllAllies || _targetType == TargetType.AllEnemies) {
            _controller.SetSelectedTarget(_targets);
            yield break;
        }

        // 单体目标默认选择一个
        SelectedTarget(_currentIndex);
        yield break;
    }

    public override IEnumerator Execute() {
        while (true) {
            // 防止输入响应过快
            if (_ignoreConfirmThisFrame) {
                _ignoreConfirmThisFrame = false;
                yield return null;
                continue;
            }

            // 选择目标
            HandleInput();

            // 确定目标
            if (InputSystemController.Instance.GetUISubmitPressed()) {
                ConfirmSelection();
                yield break;
            }

            // 取消选择，返回命令选择状态
            if (InputSystemController.Instance.GetUICancelPressed()) {
                _controller.SetState(new PlayerInputState(_controller));
                yield break;
            }

            yield return null;
        }
    }

    public override IEnumerator Exit() {
        _controller.ClearTargetSelection();
        yield break;
    }

    /// <summary>
    /// 处理玩家选择目标输入并实时更改选择的目标
    /// </summary>
    private void HandleInput() {
        if (_navigateCooldown > 0) {
            _navigateCooldown -= Time.deltaTime;
            return;
        }

        Vector2 navigate = InputSystemController.Instance.GetNavigateInput();
        if (Mathf.Abs(navigate.x) <= 0.5f && Mathf.Abs(navigate.y) < 0.5f) {
            return;
        }

        int step = navigate.x > 0 || navigate.y > 0 ? 1 : -1;
        int nextIndex = (_currentIndex + step + _targets.Count) % _targets.Count;
        SelectedTarget(nextIndex);
        _navigateCooldown = InputCooldownTime;
    }

    /// <summary>
    /// 根据序列值选择单体目标
    /// </summary>
    /// <param name="index"></param>
    private void SelectedTarget(int index) {
        if (_currentIndex < 0 || _currentIndex >= _targets.Count) {
            return;
        }

        _currentIndex = index;
        _controller.SetSelectedTarget(_targets[_currentIndex]);
    }

    /// <summary>
    /// 尝试确认选择的单体目标
    /// </summary>
    /// <returns>选择成功</returns>
    private bool ConfirmSelection() {
        if (_targetType == TargetType.AllEnemies || _targetType == TargetType.AllAllies) {
            _controller.CurrentCommandRequest.TargetEntityID = null;
        }
        else {
            _controller.CurrentCommandRequest.TargetEntityID = _targets[_currentIndex].ID;
        }

        _controller.SetState(new PerformActionState(_controller));
        return true;
    }
}
