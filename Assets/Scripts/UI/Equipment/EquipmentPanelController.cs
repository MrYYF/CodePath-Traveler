using System;

public class EquipmentPanelController : PanelController
{
    [Header("Character Tabs")]
    [SerializeField] private RectTransform tabRoot;
    [SerializeField] private EquipmentCharacterTabButton tabPrefab;
    [SerializeField] private CanvasGroup tabCanvasGroup;

    #region 运行时缓存
    private List<EquipmentCharacterTabButton> _tabButtons = new();
    private List<CharacterRuntimeData> _partyMembers = new();
    private int _memberIndex;
    #endregion

    /// <summary>
    /// 用角色运行时数据初始化人物标签
    /// </summary>
    /// <param name="partyMembers"></param>
    public void SetupWithPartyMember(List<CharacterRuntimeData> partyMembers) {
        _partyMembers.Clear();
        if (partyMembers == null || partyMembers.Count == 0) return;

        for (int i = 0; i < partyMembers.Count; i++) {
            CharacterRuntimeData member = partyMembers[i];
            if (member == null) continue;
            _partyMembers.Add(member);
        }

        BuildCharacterTabs();
        ClampMemberIndex();
    }

    /// <summary>
    /// 生成人物标签
    /// </summary>
    private void BuildCharacterTabs() {
        ClearTabButtons();

        for (int i = 0; i < _partyMembers.Count; i++) {
            EquipmentCharacterTabButton tab = Instantiate(tabPrefab, tabRoot);
            tab.Setup(_partyMembers[i], i, onTabSelected, onTabClicked);
            tab.SetSelectedVisual(i==_memberIndex); 
            _tabButtons.Add(tab);
        }
    }

    private void onTabClicked(int index) {
        throw new NotImplementedException();
    }

    private void onTabSelected(int index) {
        _memberIndex = index;
    }

    /// <summary>
    /// 清空标签按钮以及列表
    /// </summary>
    private void ClearTabButtons() {
        for (int i = 0; i < _tabButtons.Count; i++) {
            Destroy(_tabButtons[i].gameObject);
        }

        _tabButtons.Clear();
    }

    /// <summary>
    /// 选择一个默认选中的序列值
    /// </summary>
    private void ClampMemberIndex() {
        _memberIndex = _tabButtons.Count > 0 ?
            Mathf.Clamp(_memberIndex, 0, _partyMembers.Count - 1) :
            0;
    }
}
