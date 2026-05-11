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

    }

    private void RefreshHeader(Sprite slotIcon, string slotDisplayName) {
        slotIconImage.sprite = slotIcon;
        slotNameText.text = slotDisplayName;


    }
}
