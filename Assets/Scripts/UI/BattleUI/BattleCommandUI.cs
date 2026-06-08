
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

    protected override void Awake() {
        base.Awake();
        BindPrimaryButtons();
        commandMenuCanvasGroup.gameObject.SetActive(false);
    }

    public void ShowPanel() {
        commandMenuCanvasGroup.gameObject.SetActive(true);
        btnAttack.Select();
    }

    public void ClosePanel() => commandMenuCanvasGroup.gameObject.SetActive(false);

    public void BindPrimaryButtons() {
        btnAttack.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Attack));
        btnSkill.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Skill));
        btnItem.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Item));
        btnDefend.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Defend));
        btnEscape.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Escape));
    }

    private void OnCommandClicked(BattleCommandType commandType) {
        ClosePanel();
    }
}
