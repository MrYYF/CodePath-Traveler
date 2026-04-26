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
    [SerializeField] private Button popupConfirmButton; // 确认按钮
    [SerializeField] private Button popupCancelButton; // 取消按钮

    public override Type PanelActionType => typeof(ShopAction);
    private PanelType _currentShopType;
    private ItemDefinitionSO _pendingItem;
    private ShopAction CurrentShopAction => (ShopAction)CurrentAction;


    private void Awake() {
        ReBindButtons(buyButton, OpenBuyPanel);
        ReBindButtons(sellButton, OpenSellPanel);
        ReBindButtons(popupConfirmButton, ExcuteTransaction);
        ReBindButtons(popupCancelButton, CloseConfirmPopup);
        confirmPopup.gameObject.SetActive(false);
    }

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);
        SetDefaultSelection();
        UpdateCurrencyDisplay();
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

        if(_currentShopType == PanelType.Buy) {
            SetupBuyPopup(itemDefinition);
        } else {
            SetupSellPopup(itemDefinition);
        }
    }

    private void SetupBuyPopup(ItemDefinitionSO itemDefinition) {
        bool canAfford = InventoryManager.Instance.Currency >= itemDefinition.BuyPrice;

        popupTitle.text = $"是否确认购买以下物品？";
        popupText.text = $"{itemDefinition.ItemName}\n价格：{itemDefinition.BuyPrice}";

        popupConfirmButton.interactable = canAfford;
        (canAfford ? popupConfirmButton : popupCancelButton).Select();
    }

    private void SetupSellPopup(ItemDefinitionSO itemDefinition) {
        popupTitle.text = $"是否确认出售以下物品？";
        popupText.text = $"{itemDefinition.ItemName}\n获得：{itemDefinition.SellPrice}";

        popupConfirmButton.interactable = true;
        popupConfirmButton.Select();
    }

    public override bool HandleCancelInput() {
        if (confirmPopup.gameObject.activeSelf) {
            CloseConfirmPopup();
            return true;
        }

        if (!itemPanel.gameObject.activeSelf) {
            return false;
        }

        itemPanel.gameObject.SetActive(false);
        leftPart.interactable = true;
        _pendingItem = null;
        FirstSelectedButton = _currentShopType == PanelType.Buy ? buyButton : sellButton;
        SetDefaultSelection();
        return true;
    }

    private void CloseConfirmPopup() {
        confirmPopup.gameObject.SetActive(false);
        itemPanelCanvasGroup.interactable = true;
        if (itemPanel.gameObject.activeInHierarchy) {
            itemPanel.SetDefaultSelection();
            FirstSelectedButton = itemPanel.FirstSelectedButton;
        }
    }

    public void UpdateCurrencyDisplay() {
        if (currencyAmountText == null) return;

        InventoryManager instance = InventoryManager.Instance;
        currencyAmountText.text = $"{instance.Currency}";
    }

    private void ExcuteTransaction() {
        CurrentShopAction.TryExcuteTransaction(_currentShopType, _pendingItem);
        confirmPopup.gameObject.SetActive(false);
        itemPanelCanvasGroup.interactable = true;
        UpdateCurrencyDisplay();
        if (_currentShopType == PanelType.Buy) {
            itemPanel.RefreshItemQuantity(_pendingItem);
            itemPanel.SetDefaultSelection();
            return;
        }

        int remaining = InventoryManager.Instance.GetItemQuantity(_pendingItem);
        if(remaining > 0) {
            itemPanel.RefreshItemQuantity(_pendingItem);
            itemPanel.SetDefaultSelection();
            return;
        }

        itemPanel.RemoveItemButton(_pendingItem);
    }
}
