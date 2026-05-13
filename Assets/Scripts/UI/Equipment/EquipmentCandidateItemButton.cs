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
    public Action<int> OnClick;

    public Button Button => _button;


    private void Awake() {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    public void Setup(int index, EquipmentItemSO item, int ownedCount, bool isInteractable, ItemIconSetSO iconSet) {

        _index = index;
        _button.interactable = isInteractable;

        itemNameText.text = item != null ? item.ItemName : "无装备";
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

    /// <summary>
    /// 选择时的回调函数
    /// </summary>
    /// <param name="eventData"></param>
    void ISelectHandler.OnSelect(BaseEventData eventData) {
        if (!_button.interactable) {
            return;
        }

        OnSelect.Invoke(_index);
    }

    /// <summary>
    /// 点击时的回调函数处理
    /// </summary>
    private void HandleClick() {
        if (!_button.interactable) {
            return;
        }

        OnClick.Invoke(_index);
    }
}
