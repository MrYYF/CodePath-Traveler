using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentCandidateItemButton : MonoBehaviour, ISelectHandler {
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text ownedCountText;

    [Header("Color")]
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color disabledTextColor = new Color(0.72f, 0.72f, 0.72f);

    private Button _button;
    private int _index;
    public Action<int> OnSelect;

    public Button Button => _button;


    private void Awake() {
        _button = GetComponent<Button>();
    }

    public void Setup(int index, EquipmentItemSO item, int ownedCount, bool isInteractable, ItemIconSetSO iconSet) {

        _index = index;
        _button.interactable = isInteractable;

        itemNameText.text = item != null ? item.ItemName : "ÎÞ×°±¸";
        ownedCountText.text = item != null ? ownedCount.ToString() : "-";

        if (item != null) {
            itemIcon.sprite = iconSet.GetIconForItem(item.itemIconKey);
            itemIcon.enabled = true;
        }
        else {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        itemNameText.color = isInteractable ? normalTextColor : disabledTextColor;
        ownedCountText.color = isInteractable ? normalTextColor : disabledTextColor;

    }

    void ISelectHandler.OnSelect(BaseEventData eventData) {
        
    }
}
