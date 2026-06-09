using System;
using UnityEngine.UI;

/// <summary>
/// 战斗指令UI
/// </summary>
public class BattleCommandUI : Singleton<BattleCommandUI> {
    [Header("指令UI面板")]
    [SerializeField] private CanvasGroup commandMenuCanvasGroup;

    [Header("指令UI按钮")]
    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnSkill;
    [SerializeField] private Button btnItem;
    [SerializeField] private Button btnDefend;
    [SerializeField] private Button btnEscape;

    [Header("二级菜单面板")]
    [SerializeField] private RectTransform commandDetailMenuPanel;
    [SerializeField] private SkillButton skillButtonPrefab;

    // 回调方法
    private Action<SkillDataSO> _OnSkillSelected;
    private Action<ItemDefinitionSO> _OnItemSelected;
    private Action<BattleCommandType> _onCommandSelected;

    private BattleEntity _currentEntity;
    private readonly List<GameObject> _spawnedSubMenuButtons = new List<GameObject>();

    #region 生命周期
    protected override void Awake() {
        base.Awake();
        BindPrimaryButtons();
        commandMenuCanvasGroup.gameObject.SetActive(false);
    }
    #endregion

    #region 主面板相关
    public void ShowPanel() {
        commandMenuCanvasGroup.gameObject.SetActive(true);
        btnAttack.Select();
    }

    public void ClosePanel() => commandMenuCanvasGroup.gameObject.SetActive(false);

    /// <summary>
    /// 绑定主指令按钮的点击事件
    /// </summary>
    public void BindPrimaryButtons() {
        btnAttack.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Attack));
        btnSkill.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Skill));
        btnItem.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Item));
        btnDefend.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Defend));
        btnEscape.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Escape));
    }

    /// <summary>
    /// 处理指令按钮点击事件，关闭面板并执行回调函数
    /// </summary>
    /// <param name="commandType"></param>
    private void OnCommandClicked(BattleCommandType commandType) {
        CloseAndInvoke(commandType);
    }

    /// <summary>
    /// 请求输入战斗指令
    /// </summary>
    /// <param name="entity">请求输入的实体</param>
    /// <param name="onCommandSelected">完成命令的回调</param>
    public void RequestInput(BattleEntity entity, Action<BattleCommandType> onCommandSelected) {
        _currentEntity = entity;
        _onCommandSelected = onCommandSelected;
        ShowPanel();
    }

    /// <summary>
    /// 关闭面板并执行回调函数
    /// </summary>
    /// <param name="commandType"></param>
    private void CloseAndInvoke(BattleCommandType commandType) {
        ClosePanel();
        _onCommandSelected.Invoke(commandType);
        _onCommandSelected = null;
    }
    #endregion

    #region 二级菜单面板
    /// <summary>
    /// 打开二级面板
    /// </summary>
    /// <param name="button"></param>
    private void BeginSubMenu(Button button) {
        ClearSubMenuButtons();
        commandDetailMenuPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 清空二级面板中的按钮
    /// </summary>
    private void ClearSubMenuButtons() {
        foreach (var button in _spawnedSubMenuButtons) {
            Destroy(button);
        }
        _spawnedSubMenuButtons.Clear();
    }

    /// <summary>
    /// 外部调用入口
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="onCommandSelected"></param>
    /// <param name="onSkillSelected"></param>
    /// <param name="onItemSelected"></param>
    public void RequestInput(BattleEntity entity,
        Action<BattleCommandType> onCommandSelected,
        Action<SkillDataSO> onSkillSelected,
        Action<ItemDefinitionSO> onItemSelected) {
        _currentEntity = entity;
        _onCommandSelected = onCommandSelected;
        _OnSkillSelected = onSkillSelected;
        _OnItemSelected = onItemSelected;
        ShowPanel();
    }
    #endregion
}
