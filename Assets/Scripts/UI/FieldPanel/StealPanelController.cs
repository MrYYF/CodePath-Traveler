


using System;
using TMPro;
using UnityEngine.UI;

public class StealPanelController : PanelController
{
    [Header("Steal Panel")]
    [SerializeField] private StealItemButton stealItemButtonPrefab; // 偷窃物品按钮预制体
    [SerializeField] private RectTransform contentRoot; // 物品按钮的父对象

    [Header("Confirm Popup")]
    [SerializeField] private RectTransform confirmPopup; // 确认按钮
    [SerializeField] private TMP_Text popupText; // 确认弹窗文本
    [SerializeField] private Button confirmButton; // 确认按钮
    [SerializeField] private Button cancelButton; // 取消按钮

    public override Type PanelActionType => typeof(StealAction);
}
