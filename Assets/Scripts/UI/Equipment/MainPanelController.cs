

using System;
using TMPro;
using UnityEngine.UI;

public class MainPanelController : PanelController {
    [Header("Main Panel")]

    [SerializeField] private Button itemButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private TMP_Text currencyAmountText;

    [SerializeField] private ItemPanelController itemPanelController;
    [SerializeField] private EquipmentPanelController equipmentPanelController;
    [SerializeField] private CanvasGroup leftPartCanvasGroup;

    private GameObject _currentOpenPanel;
    private Button _currentSelectedButton;

    private void Awake() {
        ReBindButtons(itemButton, OpenItemPanel);
        ReBindButtons(equipmentButton, OpenEquipmentPanel);
    }

    private void OnEnable() {
        FirstSelectedButton = itemButton;
        SetDefaultSelection();
        UpdateCurrencyDisplay();
    }

    private void OnDisable() {
        if (_currentOpenPanel != null) {
            _currentOpenPanel.SetActive(false);
            _currentOpenPanel = null;
        }
        leftPartCanvasGroup.interactable = true;
    }

    public override bool HandleCancelInput() {
        if (_currentOpenPanel == null || _currentOpenPanel.activeSelf == false) {
            return false;
        }

        if (_currentOpenPanel != null &&
            _currentOpenPanel == equipmentPanelController.gameObject &&
            equipmentPanelController.HandleCancelInput()) {
            return true;
        }

        CloseCurrentPanel();
        return true;
    }

    /// <summary>
    /// 关闭当前启动的二级面板
    /// </summary>
    private void CloseCurrentPanel() {
        FirstSelectedButton = _currentSelectedButton;
        _currentOpenPanel.SetActive(false);
        _currentOpenPanel = null;
        leftPartCanvasGroup.interactable = true;
        SetDefaultSelection();
    }

    /// <summary>
    /// 打开物品面板并用PanelType.Item初始化物品面板控制器。
    /// </summary>
    private void OpenItemPanel() {
        OpenPanel(itemPanelController.gameObject, itemButton);
        itemPanelController.SetupPanel(PanelType.Item);
    }

    /// <summary>
    /// 打开装备面板并用当前的队伍成员初始化装备面板控制器。
    /// </summary>
    private void OpenEquipmentPanel() {
        OpenPanel(equipmentPanelController.gameObject, equipmentButton);
        equipmentPanelController.SetupWithPartyMember(PartyManager.Instance != null ? PartyManager.Instance.PartyMembers : null);
    }

    /// <summary>
    /// 打开指定面板的方法，接受一个面板对象和一个按钮对象作为参数。该方法将当前打开的面板和选中的按钮更新为传入的参数，并将面板设置为可见，同时禁用左侧部分的交互，以确保用户只能与当前打开的面板进行交互。
    /// </summary>
    /// <param name="panel">面板</param>
    /// <param name="button">按钮</param>
    private void OpenPanel(GameObject panel, Button button) {
        _currentOpenPanel = panel;
        _currentSelectedButton = button;

        panel.SetActive(true);
        leftPartCanvasGroup.interactable = false;
    }

    /// <summary>
    /// 更新货币显示文本
    /// </summary>
    public void UpdateCurrencyDisplay() {
        if (currencyAmountText == null) return;

        InventoryManager instance = InventoryManager.Instance;
        currencyAmountText.text = $"{instance.Currency}";
    }
}
