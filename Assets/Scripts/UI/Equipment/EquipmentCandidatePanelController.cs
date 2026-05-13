using System;
using TMPro;
using UnityEngine.UI;

public class EquipmentCandidatePanelController : MonoBehaviour
{
    [Header("Candidate Panel")]
    [SerializeField] private Image slotIconImage;
    [SerializeField] private TMP_Text slotNameText;

    [SerializeField] private RectTransform candidateListRoot;
    [SerializeField] private EquipmentCandidateItemButton candidateButtonPrefab;

    private readonly List<EquipmentCandidateItemButton> _buttons = new();
    private readonly List<EquipmentItemSO> _candidates = new();

    public event Action<int> OnSelectedIndexChanged;
    public event Action<int> OnCandidateClicked;
    public EquipmentItemSO GetCandidate(int index) => _candidates[index];
    public Button GetFirstButton() => _buttons[0].Button;

    /// <summary>
    /// 打开对应装备槽位可选装备面板
    /// </summary>
    /// <param name="slot">装备槽位</param>
    /// <param name="member">人物运行时数据</param>
    /// <param name="slotDisplayName">槽位展示名称</param>
    /// <param name="slotIcon">槽位图标</param>
    public void OpenForSlot(EquipSlot slot, CharacterRuntimeData member, string slotDisplayName, Sprite slotIcon) {
        RefreshHeader(slotIcon, slotDisplayName);
        RebuildCandidates(slot);
    }

    /// <summary>
    /// 重建备选装备列表
    /// </summary>
    /// <param name="slot">装备槽位</param>
    private void RebuildCandidates(EquipSlot slot) {
        _candidates.Clear();
        ClearButtons();

        InventoryManager inventory = InventoryManager.Instance;
        PartyManager party = PartyManager.Instance;

        // 待选装备列表
        _candidates.AddRange(EquipmentService.BuildCandidates(inventory,slot));

        for (int i = 0; i < _candidates.Count; i++) {
            int availableCount = EquipmentService.GetAvailableItemCount(inventory, party, _candidates[i]);
            bool isInteractable = _candidates[i] == null || availableCount > 0;
            EquipmentCandidateItemButton candidateButton = Instantiate(candidateButtonPrefab, candidateListRoot);
            candidateButton.Setup(i, _candidates[i], availableCount, isInteractable, inventory.IconSet);
            candidateButton.OnClick += HandleCandidateClicked;
            candidateButton.OnSelect += HandleCandidateSelected;
            _buttons.Add(candidateButton);
        }
    }

    /// <summary>
    /// 刷新待选装备面板的头部信息
    /// </summary>
    /// <param name="slotIcon">槽位图标</param>
    /// <param name="slotDisplayName">槽位名称</param>
    private void RefreshHeader(Sprite slotIcon, string slotDisplayName) {
        slotIconImage.sprite = slotIcon;
        slotNameText.text = slotDisplayName;
    }

    public void Close() {
        _candidates.Clear();
        ClearButtons();
    }

    private void ClearButtons() {
        for (int i = 0; i < _buttons.Count; i++) {
            if(_buttons[i] != null ) {
                Destroy(_buttons[i].gameObject);
            }
        }

        _buttons.Clear();
    }

    /// <summary>
    /// 从上一级面板（EquipmentPanelController）获得的点击可选装备的回调函数
    /// </summary>
    /// <param name="index">可选装备的序列值</param>
    private void HandleCandidateClicked(int index) {
        OnCandidateClicked?.Invoke(index);
    }

    /// <summary>
    /// 从上一级面板（EquipmentPanelController）获得的选择可选装备的回调函数
    /// </summary>
    /// <param name="index">可选装备的序列值</param>
    private void HandleCandidateSelected(int index) {
        OnSelectedIndexChanged?.Invoke(index);
    }
}
