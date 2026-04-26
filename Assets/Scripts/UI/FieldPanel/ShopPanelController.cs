using System;
using TMPro;
using UnityEngine.UI;

public class ShopPanelController : PanelController
{
    [Header("一级按钮与金额")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private TMP_Text currencyAmountText;

    [Header("二级列表")]
    [SerializeField] private ItemPanelController itemPanel; // 商品列表面板

    [Header("交互区域")]
    [SerializeField] private CanvasGroup leftPart;
    [SerializeField] private CanvasGroup itemPanelCanvasGroup;

    [Header("Confirm Popup")]
    [SerializeField] private RectTransform confirmPopup; // 确认弹窗
    [SerializeField] private TMP_Text popupTitle; // 确认弹窗标题文本
    [SerializeField] private TMP_Text popupText; // 确认弹窗文本
    [SerializeField] private Button confirmButton; // 确认按钮
    [SerializeField] private Button cancelButton; // 取消按钮

    public override Type PanelActionType => typeof(ShopAction);
    private PanelType _currentShopType;
    private ItemDefinitionSO _pendingItem;


    private void Awake() {
        ReBindButtons(buyButton, OpenBuyPanel);
        ReBindButtons(sellButton, OpenSellPanel);
        confirmPopup.gameObject.SetActive(false);
    }

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);
        SetDefaultSelection();
    }

    private void OpenBuyPanel() {
        OpenItemPanel(PanelType.Buy);
    }

    private void OpenSellPanel() {
        OpenItemPanel(PanelType.Sell);
    }

    private void OpenItemPanel(PanelType panelType) {
        _currentShopType = panelType;
        leftPart.interactable = false;
        itemPanel.gameObject.SetActive(true);

        itemPanel.SetupPanel(panelType,CurrentAction,OpenConfirmPopup);
    }

    private void OpenConfirmPopup(ItemDefinitionSO itemDefinition) {
        _pendingItem = itemDefinition;
        confirmPopup.gameObject.SetActive(true);
        itemPanelCanvasGroup.interactable = false;
    }

    public override bool HandleCancelInput() {
        if(confirmPopup.gameObject.activeSelf) {
            confirmPopup.gameObject.SetActive(false);
            itemPanelCanvasGroup.interactable = true;
            if(itemPanel.gameObject.activeInHierarchy) {
                itemPanel.SetDefaultSelection();
                FirstSelectedButton = itemPanel.FirstSelectedButton;
            }
            return true;
        }

        if(!itemPanel.gameObject.activeSelf) {
            return false;
        }

        itemPanel.gameObject.SetActive(false);
        leftPart.interactable = true;
        _pendingItem = null;
        FirstSelectedButton = _currentShopType == PanelType.Buy ? buyButton : sellButton;
        SetDefaultSelection();
        return true;
    }

}
