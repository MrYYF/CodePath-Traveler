using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class EquipmentSlotButton : MonoBehaviour, ISelectHandler {

    [SerializeField] private Image slotIconImage;
    [SerializeField] private TMP_Text slotNameText;

    private CanvasGroup _canvasGroup;
    private Button _button;
    private int _index;
    private Action<int> _onTabSelected;
    private Action<int> _onTabClicked;
    public Button Button {
        get {
            if(_button == null) {
                _button = GetComponent<Button>();
                _defaultNavigation = _button.navigation;
            }
            return _button;
        }
    }

    private bool _isSlotUsable = true;
    private bool _isInputEnabled = true;
    private Navigation _defaultNavigation;
    public Sprite SlotIconSprite => slotIconImage.sprite;

    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();
        Button.onClick.AddListener(HandleClicked);
    }

    private void OnDisable() {
        _button.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 初始化按钮
    /// </summary>
    /// <param name="equipmentItem">装备数据</param>
    /// <param name="index">对应槽位序列号</param>
    /// <param name="onTabSelected">选中事件回调</param>
    /// <param name="onTabClicked">点击事件回调</param>
    /// <param name="isSlotUsable">槽位是否可用</param>
    public void SetupButton(
        EquipmentItemSO equipmentItem, 
        int index, 
        Action<int> onTabSelected, 
        Action<int> onTabClicked,
        bool isSlotUsable) {
        _index = index;
        _onTabSelected = onTabSelected;
        _onTabClicked = onTabClicked;
        _isSlotUsable = isSlotUsable;

        string displayName = equipmentItem != null ? equipmentItem.ItemName : "未装备";
        slotNameText.text = displayName;

        ApplyButtonInteractableState();
    }

    public void SetInputEnabled(bool isEnabled) {
        _isInputEnabled = isEnabled;
        ApplyButtonInteractableState();
    }

    /// <summary>
    /// 根据按钮是否可以互动设置相关样式以及功能
    /// </summary>
    private void ApplyButtonInteractableState() {
        bool isInteractable = _isSlotUsable && _isInputEnabled;
        Button.interactable = isInteractable;
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.alpha = isInteractable ? 1f : 0f;

        Navigation navigation = _defaultNavigation;

        if(!isInteractable) {
            navigation.mode = Navigation.Mode.None;
        }
        Button.navigation = navigation;
    }

    private void HandleClicked() {
        if (_button == null || !_button.interactable) return;
        _onTabClicked?.Invoke(_index);
    }

    public void OnSelect(BaseEventData eventData) {
        if (_button == null || !_button.interactable) return;

        _onTabSelected?.Invoke(_index);
    }
}
