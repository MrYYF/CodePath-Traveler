using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentCandidatePanelController : MonoBehaviour
{
    [Header("")]
    [SerializeField] private Image slotIconImage;
    [SerializeField] private TMP_Text slotNameText;

    [SerializeField] private RectTransform candidateListRoot;
    [SerializeField] private EquipmentCandidateItemButton candidateButtonPrefab;

    private readonly List<EquipmentCandidateItemButton> _buttons = new();
    private readonly List<EquipmentItemSO> _candidates = new();

    public void OpenForSlot(EquipSlot slot, CharacterRuntimeData member, string slotDisplayName, Sprite slotIcon) {
        RefreshHeader(slotIcon, slotDisplayName);
        RebuildCandidates(slot);
    }

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
            _buttons.Add(candidateButton);
        }
    }

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
}
