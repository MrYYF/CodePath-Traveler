using System;
using TMPro;

public class EquipmentPanelController : PanelController {
    [Header("Title")]
    [SerializeField] private TMP_Text memberNameText;

    [Header("Character Tabs")]
    [SerializeField] private RectTransform tabRoot;
    [SerializeField] private EquipmentCharacterTabButton tabPrefab;
    [SerializeField] private CanvasGroup tabCanvasGroup;

    [Header("Slot Buttons")]
    [SerializeField] private EquipmentSlotButton[] slotButtons;
    [SerializeField] private CanvasGroup slotListCanvasGroup;

    [Header("Left Category Name")]
    [SerializeField] private TMP_Text[] leftCategoryNameTexts;
    [SerializeField] private Color leftCategoryEnableColor = Color.black;
    [SerializeField] private Color leftCategoryDisableColor = new Color(0.72f, 0.72f, 0.72f);

    [Header("Stat Preview")]
    [SerializeField] private EquipmentStatPreviewPanel statPreviewPanel;

    [Header("Candidate Panel")]
    [SerializeField] private GameObject candidatePanelRoot;
    [SerializeField] private EquipmentCandidatePanelController candidatePanelController;

    #region 运行时缓存
    private List<EquipmentCharacterTabButton> _tabButtons = new();
    private List<CharacterRuntimeData> _partyMembers = new();
    private int _memberIndex;
    private int _slotIndex;
    private CharacterRuntimeData CurrentMember =>
        _memberIndex >= 0 && _memberIndex < _partyMembers.Count ?
        _partyMembers[_memberIndex] :
        null;
    #endregion
    #region 固定属性
    private readonly WeaponType[] WeaponSlotTypes = {
        WeaponType.Dagger,
        WeaponType.Sword,
        WeaponType.Spear,
        WeaponType.Axe,
        WeaponType.Bow,
        WeaponType.Staff
    };
    private readonly EquipSlot[] FixedSlotOrder = {
        EquipSlot.Dagger,
        EquipSlot.Sword,
        EquipSlot.Spear,
        EquipSlot.Axe,
        EquipSlot.Bow,
        EquipSlot.Staff,
        EquipSlot.Head,
        EquipSlot.Body,
        EquipSlot.Shield,
        EquipSlot.Accessory1,
        EquipSlot.Accessory2
    };
    private enum FocusLayer {
        CharacterTabs,
        SlotList,
        CandidateList
    }
    #endregion
    private FocusLayer _focusLayer = FocusLayer.CharacterTabs;

    #region tab人物标签相关

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
        RefreshCurrentMemberView();
        EnterCharacterTabLayer();
    }

    /// <summary>
    /// 生成人物标签
    /// </summary>
    private void BuildCharacterTabs() {
        ClearTabButtons();

        for (int i = 0; i < _partyMembers.Count; i++) {
            EquipmentCharacterTabButton tab = Instantiate(tabPrefab, tabRoot);
            tab.Setup(_partyMembers[i], i, OnTabSelected, OnTabClicked);
            tab.SetSelectedVisual(i == _memberIndex);
            _tabButtons.Add(tab);
        }
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

    /// <summary>
    /// 刷新当前标签人物的装备页面，同时更新人物可用装备槽位以及视觉显示效果
    /// </summary>
    private void RefreshCurrentMemberView() {
        memberNameText.text = CurrentMember?.DisplayName;

        for (int i = 0; i < FixedSlotOrder.Length; i++) {
            EquipmentSlotButton equipmentSlotButton = slotButtons[i];
            EquipSlot equipSlot = FixedSlotOrder[i];
            EquipmentItemSO equipmentItem = CurrentMember?.GetEquippedItem(equipSlot);

            equipmentSlotButton.gameObject.SetActive(true);
            equipmentSlotButton.SetupButton(equipmentItem, i, OnSlotSelected, OnSlotClicked, IsSlotUsableForMember(CurrentMember, i));
        }

        OnSlotSelected(_memberIndex);
        UpdateTabSelectionVisual();
        RefreshLeftCategoryColors(CurrentMember);
    }

    private void RefreshLeftCategoryColors(CharacterRuntimeData member) {
        for (int i = 0; i < leftCategoryNameTexts.Length; i++) {
            leftCategoryNameTexts[i].color = IsSlotUsableForMember(member, i) ?
                leftCategoryEnableColor :
                leftCategoryDisableColor;
        }
    }

    /// <summary>
    /// 判断人物能否装备某个槽位对应的装备类型
    /// </summary>
    /// <param name="member">成员数据</param>
    /// <param name="slotIndex">槽位对应序列值</param>
    /// <returns></returns>
    private bool IsSlotUsableForMember(CharacterRuntimeData member, int slotIndex) {
        //非武器槽位默认可以
        if (slotIndex >= WeaponSlotTypes.Length)
            return true;

        return member.Definition is AllyDefinitionSO ally && ally.CanEquipWeaponType(WeaponSlotTypes[slotIndex]);
    }

    /// <summary>
    /// 进入角色标签层
    /// </summary>
    private void EnterCharacterTabLayer() {
        SetCharacterTabInteractable(true);
        UpdateTabSelectionVisual();
        SetSlotListInteractable(false);
        _memberIndex = Mathf.Clamp(_memberIndex, 0, _partyMembers.Count - 1);
        FirstSelectedButton = _tabButtons[_memberIndex].Button;
        SetDefaultSelection();
        _focusLayer = FocusLayer.CharacterTabs;
    }

    /// <summary>
    /// 设置角色标签是否可互动
    /// </summary>
    /// <param name="interactable">是否可互动</param>
    private void SetCharacterTabInteractable(bool interactable) {
        tabCanvasGroup.interactable = interactable;

        for (int i = 0; i < _tabButtons.Count; i++) {
            _tabButtons[i].Button.interactable = interactable;
        }
    }

    /// <summary>
    /// 标签被点击时的回调函数
    /// </summary>
    /// <param name="index"></param>
    private void OnTabClicked(int index) {
        OnTabSelected(index);
        EnterSlotLayer();
    }

    /// <summary>
    /// 标签被选择时的回调函数
    /// </summary>
    /// <param name="index"></param>
    private void OnTabSelected(int index) {
        _memberIndex = index;
        RefreshCurrentMemberView();
    }

    /// <summary>
    /// 更新标签选中的视觉状态
    /// </summary>
    private void UpdateTabSelectionVisual() {
        for (int i = 0; i < _tabButtons.Count; i++) {
            _tabButtons[i].SetSelectedVisual(i == _memberIndex);
        }
    }
    #endregion

    #region 装备槽位相关
    /// <summary>
    /// 进入装备槽位选择层
    /// </summary>
    private void EnterSlotLayer() {
        CloseCandidateListInternal();
        SetCharacterTabInteractable(false);
        SetSlotListInteractable(true);
        EnsureSelectedSlotValid();
        if (_slotIndex >= slotButtons.Length) return;
        FirstSelectedButton = slotButtons[_slotIndex].Button;
        SetDefaultSelection();
        _focusLayer = FocusLayer.SlotList;
    }

    /// <summary>
    /// 设置装备槽位是否可互动
    /// </summary>
    /// <param name="interactable"></param>
    private void SetSlotListInteractable(bool interactable) {
        slotListCanvasGroup.interactable = interactable;

        for (int i = 0; i < slotButtons.Length; i++) {
            slotButtons[i].SetInputEnabled(IsSlotUsableForMember(CurrentMember,i));
        }
    }

    /// <summary>
    /// 选择可用的装备槽位序列值
    /// </summary>
    private void EnsureSelectedSlotValid() {
        if (IsSlotIndexUsabel(_slotIndex))
            return;

        for (int i = 0; i < FixedSlotOrder.Length; i++) {
            if (IsSlotIndexUsabel(i)) {
                _slotIndex = i;
                return;
            }
        }

        _slotIndex = -1;
    }

    /// <summary>
    /// 判断指定序号的装备槽位是否可用
    /// </summary>
    /// <param name="index">装备槽位序列值</param>
    /// <returns>是否可用</returns>
    private bool IsSlotIndexUsabel(int index) =>
        index >= 0 &&
        index < slotButtons.Length &&
        IsSlotUsableForMember(CurrentMember, index);

    /// <summary>
    /// 装备槽位选中回调函数
    /// </summary>
    /// <param name="index">装备槽位序列值</param>
    private void OnSlotSelected(int index) {
        if (CurrentMember == null || statPreviewPanel == null)
            return;
        statPreviewPanel.Refresh(CurrentMember.GetTotalStats(), CurrentMember.GetTotalStats(), false);
    }

    /// <summary>
    /// 装备槽位点击回调函数
    /// </summary>
    /// <param name="index">装备槽位序列值</param>
    private void OnSlotClicked(int index) {
        OnSlotSelected(index);
        OpenCandidateList(index);
    }


    #endregion

    #region 可选装备面板
    /// <summary>
    /// 打开待选装备列表面板
    /// </summary>
    /// <param name="index"></param>
    private void OpenCandidateList(int index) {
        SetSlotListInteractable(false);
        SetCharacterTabInteractable(false);

        candidatePanelRoot.SetActive(true);
        EquipSlot slot = FixedSlotOrder[index];
        string slotName = leftCategoryNameTexts[index].text;
        candidatePanelController.OpenForSlot(slot, CurrentMember, slotName, slotButtons[index].SlotIconSprite);
        _focusLayer = FocusLayer.CandidateList;
    }

    /// <summary>
    /// 关闭待选装备列表面板
    /// </summary>
    private void CloseCandidateListInternal() {
        candidatePanelController.Close();
        candidatePanelRoot.SetActive(false);
    }
    #endregion

    public override bool HandleCancelInput() {
        if (candidatePanelRoot.activeInHierarchy) {
            EnterSlotLayer();
            return true;
        }

        if(_focusLayer == FocusLayer.SlotList) {
            EnterCharacterTabLayer();
            return true;
        }

        return false;
    }
}
