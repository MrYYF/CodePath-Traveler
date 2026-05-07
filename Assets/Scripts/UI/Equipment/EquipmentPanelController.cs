using System;
using TMPro;

public class EquipmentPanelController : PanelController
{
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

    #region 运行时缓存
    private List<EquipmentCharacterTabButton> _tabButtons = new();
    private List<CharacterRuntimeData> _partyMembers = new();
    private int _memberIndex;
    private CharacterRuntimeData CurrentMember => 
        _memberIndex >=0 && _memberIndex<_partyMembers.Count ? 
        _partyMembers[_memberIndex] : 
        null;
    #endregion
    #region 固定槽位顺序
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
    #endregion

    /// <summary>
    /// 刷新当前标签人物的装备页面
    /// </summary>
    private void RefreshCurrentMemberView() {
        memberNameText.text = CurrentMember?.DisplayName;

        for (int i = 0; i < FixedSlotOrder.Length; i++) {
            EquipmentSlotButton equipmentSlotButton = slotButtons[i];
            EquipSlot equipSlot = FixedSlotOrder[i];
            EquipmentItemSO equipmentItem = CurrentMember?.GetEquippedItem(equipSlot);

            equipmentSlotButton.gameObject.SetActive(true);
            equipmentSlotButton.SetupButton(equipmentItem, i, null, null, IsSlotUsableForMember(CurrentMember, i));
            Debug.Log($"{FixedSlotOrder[i].ToString()} 是否可以被装备：{IsSlotUsableForMember(CurrentMember, i)}");
        }
    }

    /// <summary>
    /// 判断人物能否装备某个槽位对应的装备类型
    /// </summary>
    /// <param name="member">成员数据</param>
    /// <param name="slotIndex">槽位对应序列值</param>
    /// <returns></returns>
    private bool IsSlotUsableForMember(CharacterRuntimeData member,int slotIndex) {
        //非武器槽位默认可以
        if(slotIndex >= WeaponSlotTypes.Length) 
            return true;

        return member.Definition is AllyDefinitionSO ally && ally.CanEquipWeaponType(WeaponSlotTypes[slotIndex]);
    } 

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
