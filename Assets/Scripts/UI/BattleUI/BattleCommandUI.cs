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
    [SerializeField] private ItemButton itemButtonPrefab;

    // 回调方法
    private Action<SkillDataSO> _onSkillSelected;
    private Action<ItemDefinitionSO> _onItemSelected;
    private Action<BattleCommandType> _onCommandSelected;

    private BattleEntity _currentEntity;
    private readonly List<GameObject> _spawnedSubMenuButtons = new List<GameObject>();
    private Button _lastPrimaryButton; // 一级菜单选中按钮缓存
    private bool _subMenuOpen; // 二级菜单打开状态

    #region 生命周期
    protected override void Awake() {
        base.Awake();
        BindPrimaryButtons();
        commandMenuCanvasGroup.gameObject.SetActive(false);
    }

    private void Update() {
        if (!commandDetailMenuPanel.gameObject.activeSelf) {
            return;
        }

        InputSystemController input = InputSystemController.Instance;
        if (input != null && input.GetUICancelPressed() && _subMenuOpen) {
            CloseSubMenu();
        }
    }
    #endregion

    #region 主面板相关
    public void ShowPanel() {
        commandMenuCanvasGroup.gameObject.SetActive(true);
        btnAttack.Select();
    }

    public void ClosePanel() {
        CloseSubMenu();
        commandMenuCanvasGroup.gameObject.SetActive(false);
    }

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
        switch (commandType) {
            case BattleCommandType.Skill:
                OpenSkillMenu();
                break;
            case BattleCommandType.Item:
                OpenItemMenu();
                break;
            default:
                CloseAndInvoke(commandType);
                break;
        }
    }

    /// <summary>
    /// 请求输入战斗指令
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
        _onSkillSelected = onSkillSelected;
        _onItemSelected = onItemSelected;
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
    /// 打开二级技能面板
    /// </summary>
    private void OpenSkillMenu() {
        int currentSP = _currentEntity.CurrentSP;
        List<SkillDataSO> skills = _currentEntity.Definition.InitialSkills;
        if (skills.Count <= 0) {
            return;
        }
        BeginSubMenu(btnSkill);

        // 构建技能列表
        Button firstButton = null;
        foreach (var skill in skills) {
            if (skill == null) {
                continue;
            }
            SkillButton skillButton = Instantiate(skillButtonPrefab, commandDetailMenuPanel);
            skillButton.Setup(skill);
            Button button = skillButton.GetComponent<Button>();
            button.interactable = skill.spCost <= currentSP;
            button.onClick.AddListener(() => OnSkillButtonClick(skill));

            if (firstButton == null && button.interactable) {
                firstButton = button;
            }

            _spawnedSubMenuButtons.Add(skillButton.gameObject);
        }

        if (_spawnedSubMenuButtons.Count <= 0) {
            CloseSubMenu();
            return;
        }
        firstButton?.Select();
    }

    private void OpenItemMenu() {
        BeginSubMenu(btnItem);
        InventoryManager inventory = InventoryManager.Instance;
        Button firstButton = null;

        foreach (var item in inventory.CurrentInventory) {
            if (!item.IsConsumable) {
                continue;
            }

            ItemButton itemButton = Instantiate(itemButtonPrefab, commandDetailMenuPanel);
            itemButton.SetupButton(item, OnItemButtonClick);

            if (firstButton == null && itemButton.CurrentButton) {
                firstButton = itemButton.CurrentButton;
            }

            _spawnedSubMenuButtons.Add(itemButton.gameObject);
        }

        // 没有可选项
        if (_spawnedSubMenuButtons.Count <= 0) {
            CloseSubMenu();
            return;
        }
        firstButton?.Select();
    }

    /// <summary>
    /// 打开二级面板
    /// </summary>
    /// <param name="returnButton">返回时需要选中的命令按钮</param>
    private void BeginSubMenu(Button returnButton) {
        ClearSubMenuButtons();
        commandDetailMenuPanel.gameObject.SetActive(true);
        _subMenuOpen = true;
        _lastPrimaryButton = returnButton;
        commandMenuCanvasGroup.interactable = false;
    }

    /// <summary>
    /// 关闭二级面板
    /// </summary>
    /// <param name="restorePrimarySelection">是否返回上一级菜单</param>
    private void CloseSubMenu(bool restorePrimarySelection = true) {
        ClearSubMenuButtons();
        commandDetailMenuPanel.gameObject.SetActive(false);
        commandMenuCanvasGroup.interactable = true;

        if (restorePrimarySelection && _subMenuOpen) {
            _lastPrimaryButton.Select();
            _lastPrimaryButton = null;
            _subMenuOpen = false;
        }
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

    #endregion

    #region 按钮回调
    private void OnSkillButtonClick(SkillDataSO skill) {
        ClosePanel();
        _onSkillSelected?.Invoke(skill);
        _onSkillSelected = null;
    }

    private void OnItemButtonClick(ItemDefinitionSO item) {
        ClosePanel();
        _onItemSelected?.Invoke(item);
        _onSkillSelected = null;
    }
    #endregion
}
