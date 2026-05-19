

/// <summary>
/// 交互基类，负责管理当前交互对象、可用指令列表缓存，以及发布交互状态变化事件
/// 决定被挂载对象是否可以被交互
/// </summary>
public class InteractionBase : MonoBehaviour
{
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

    private void Awake() {
        CacheActions();
        HeadAnchor = transform.GetChild(0);
    }

    private void OnDisable() {
        _currentInteractor = null;
        _cachedCommandInfo.Clear();
        _visibleActionEntries.Clear();
    }

    public void Interact(AllyDefinitionSO interactor) {
        EventBus.Publish(new InteractionMenuRequestEvent(this));
    }

    // 当玩家靠近交互对象时，调用此方法来更新当前交互对象和可用指令列表，并发布交互状态变化事件
    public void OnFocus(AllyDefinitionSO interactor) {
        CacheActions();
        _currentInteractor = interactor;
        RebuildCommands();
        PublishEvent(true);
    }

    // 当玩家远离交互对象时，调用此方法来清除当前交互对象和可用指令列表，并发布交互状态变化事件
    public void OnLoseFocus(AllyDefinitionSO interactor) {
        _currentInteractor = null;
        _cachedCommandInfo.Clear();
        _visibleActionEntries.Clear();
        Debug.Log("LoseFocused on " + interactor.Name + interactor.Job);

        PublishEvent(false);
        HeadAnchor.gameObject.SetActive(true);
    }

    // 获取当前交互对象的可用指令列表
    private void CacheActions() => _actionsCache = GetComponents<ActionBase>();

    // 根据当前交互对象和指令列表，构建可用指令信息列表
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

    // 检查是否有任何队伍成员可以执行该指令，只有当至少有一个队伍成员可以执行时，才将该指令添加到可用指令列表中
    private bool CanAnyPartyMemberExecute(ActionBase action) {
        var partyMembers = PartyManager.Instance.PartyMembers;
        if(partyMembers == null || partyMembers.Count == 0)
            return false;

        foreach (var member in partyMembers) {
            if(member.Definition == null)
                continue;
            if(action.CanShow(member.Definition as AllyDefinitionSO))
                return true;
        }

        return false;
    }

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
