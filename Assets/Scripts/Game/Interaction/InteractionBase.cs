

/// <summary>
/// 交互基类，负责管理当前交互对象、可用指令列表缓存，以及发布交互状态变化事件
/// 决定被挂载对象是否可以被交互
/// </summary>
public class InteractionBase : MonoBehaviour {
    [Header("Sign Trans")]
    public Transform HeadAnchor; // 头顶可互动图标的锚点

    private AllyDefinitionSO _currentInteractor; // 当前交互对象
    private ActionBase[] _actionsCache; // 当前交互对象的可用指令列表缓存
    private readonly List<ActionCommandInfo> _cachedCommandInfo = new(8); // 当前交互对象的可用指令列表缓存
    private readonly List<VisibleActionEntry> _visibleActionEntries = new(8); // 当前交互对象的可见指令列表缓存
    public IReadOnlyList<ActionCommandInfo> CachedCommandInfo => _cachedCommandInfo;

    // 用于将指令和对应的指令信息关联起来，方便后续构建可用指令列表
    private struct VisibleActionEntry {
        public ActionBase Action;
        public ActionCommandInfo CommandInfo;
    }

    #region 周期函数
    private void Awake() {
        CacheActions();
        HeadAnchor = transform.GetChild(0);
    }

    private void OnDisable() {
        _currentInteractor = null;
        _cachedCommandInfo.Clear();
        _visibleActionEntries.Clear();
    }
    #endregion

    /// <summary>
    /// 当玩家与交互对象进行交互时，调用此方法来发布交互菜单请求事件，通知UI系统显示交互菜单，并传递当前交互对象和可用指令列表信息
    /// </summary>
    /// <param name="interactor">当前交互对象</param>
    public void Interact(AllyDefinitionSO interactor) {
        EventBus.Publish(new InteractionMenuRequestEvent(this));
    }

    /// <summary>
    /// 当玩家与交互对象获得焦点时，调用此方法来设置当前交互对象，并根据当前交互对象和指令列表构建可用指令信息列表，并发布交互状态变化事件
    /// </summary>
    /// <param name="interactor">当前交互对象</param>
    public void OnFocus(AllyDefinitionSO interactor) {
        CacheActions();
        _currentInteractor = interactor;
        RebuildCommands();
        PublishEvent(true);
    }

    /// <summary>
    /// 当玩家与交互对象失去焦点时，调用此方法来清除当前交互对象和可用指令列表，并发布交互状态变化事件
    /// </summary>
    /// <param name="interactor">当前交互对象</param>
    public void OnLoseFocus(AllyDefinitionSO interactor) {
        _currentInteractor = null;
        _cachedCommandInfo.Clear();
        _visibleActionEntries.Clear();

        PublishEvent(false);
        HeadAnchor.gameObject.SetActive(true);
    }

    /// <summary>
    /// 缓存当前交互对象的可用指令列表，避免每次交互时都需要重新获取指令组件，提高性能
    /// </summary>
    private void CacheActions() => _actionsCache = GetComponents<ActionBase>();

    /// <summary>
    /// 根据当前交互对象和指令列表构建可用指令信息列表
    /// </summary>
    private void RebuildCommands() {
        _cachedCommandInfo.Clear();
        _visibleActionEntries.Clear();

        for (int i = 0; i < _actionsCache.Length; i++) {
            var action = _actionsCache[i];

            if (!CanAnyPartyMemberExecute(action))
                continue;

            _visibleActionEntries.Add(new VisibleActionEntry {
                Action = action,
                CommandInfo = action.CommandInfo
            });
        }

        if (_visibleActionEntries.Count > 1)
            _visibleActionEntries.Sort((a, b) => a.CommandInfo.Order.CompareTo(b.CommandInfo.Order));

        for (int i = 0; i < _visibleActionEntries.Count; i++) {
            _cachedCommandInfo.Add(_visibleActionEntries[i].CommandInfo);
        }

        if (_visibleActionEntries.Count > 0)
            HeadAnchor.gameObject.SetActive(false);
    }

    /// <summary>
    /// 检查当前指令是否有任何一个队伍成员可以执行，只有当至少有一个队伍成员可以执行该指令时，才会将该指令显示在交互菜单中
    /// </summary>
    /// <param name="action">当前指令</param>
    /// <returns>如果有返回true，否则返回false</returns>
    private bool CanAnyPartyMemberExecute(ActionBase action) {
        var partyMembers = PartyManager.Instance.PartyMembers;
        if (partyMembers == null || partyMembers.Count == 0)
            return false;

        foreach (var member in partyMembers) {
            if (member.Definition == null)
                continue;
            if (action.CanShow(member.Definition as AllyDefinitionSO))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 发布交互状态变化事件，通知UI系统更新交互菜单的显示状态和内容
    /// </summary>
    /// <param name="inRange">是否在交互范围内</param>
    private void PublishEvent(bool inRange) {
        EventBus.Publish(new InteractionChangedEvent(this, inRange));
    }

    #region UI回调入口
    // 当玩家在UI上选择一个指令时，调用此方法来执行对应的指令，并返回是否成功执行
    public bool ExecuteCommandFromUI(int commandIndex) {
        if (commandIndex < 0 || commandIndex >= _visibleActionEntries.Count)
            return false;

        ActionBase action = _visibleActionEntries[commandIndex].Action;

        if (!action.CanExecute(_currentInteractor))
            return false;

        action.TriggerAction(_currentInteractor);
        return true;
    }
    #endregion
}
