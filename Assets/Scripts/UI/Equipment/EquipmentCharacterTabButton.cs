using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentCharacterTabButton : MonoBehaviour, ISelectHandler {
    [SerializeField] private Image portraitImage;
    [SerializeField] private CanvasGroup canvasGroup;

    private Button _button;

    private int _index;

    private Action<int> _onTabSelected;
    private Action<int> _onTabClicked;

    public Button Button => _button;

    private void Awake() {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClicked);
        SetSelectedVisual(false);
    }

    public void Setup(CharacterRuntimeData member, int index, Action<int> onTabSelected, Action<int> onTabClicked) {
        _index = index;
        _onTabSelected = onTabSelected;
        _onTabClicked = onTabClicked;

        if (portraitImage == null) return;

        portraitImage.sprite = member.Definition.Portrait;
    }

    /// <summary>
    /// 设置标签是否被选中
    /// </summary>
    /// <param name="isSelected">标签是否被选中</param>
    public void SetSelectedVisual(bool isSelected) {
        canvasGroup.alpha = isSelected ? 1f : 0.5f;
    }

    public void SetInteractable(bool isInteractable) {
        _button.interactable = isInteractable;
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
