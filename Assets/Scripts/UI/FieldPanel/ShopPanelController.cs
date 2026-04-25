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
    [SerializeField] private ItemPanelController itemPanel;

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


}
