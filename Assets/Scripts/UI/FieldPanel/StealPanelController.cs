


using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class StealPanelController : PanelController
{
    [Header("Steal Panel")]
    [SerializeField] private StealItemButton stealItemButtonPrefab; // 偷窃物品按钮预制体
    [SerializeField] private RectTransform contentRoot; // 物品按钮的父对象

    [Header("Confirm Popup")]
    [SerializeField] private RectTransform confirmPopup; // 确认弹窗
    [SerializeField] private TMP_Text popupText; // 确认弹窗文本
    [SerializeField] private Button confirmButton; // 确认按钮
    [SerializeField] private Button cancelButton; // 取消按钮

    private readonly List<StealItemButton> _stealItemButtons = new();
    public override Type PanelActionType => typeof(StealAction);
    private StealAction CurrentStealAction => (StealAction)CurrentAction;

    // 当前选中的物品
    private ItemDefinitionSO _pendingItem;

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);

        gameObject.SetActive(true);

        confirmPopup.gameObject.SetActive(false);
        RefreshItemList();
    }

    private void RefreshItemList() {
        ClearItemList();
        foreach (InventoryItem item in CurrentStealAction.stealableItems) {
            StealItemButton button = Instantiate(stealItemButtonPrefab, contentRoot);
            button.SetupButton(item, OpenConfirmPopup);
            _stealItemButtons.Add(button);
        }
        FirstSelectedButton = _stealItemButtons[0].CurrentButton;

        SetDefaultSelection();
    }

    /// <summary>
    /// 打开二次确认弹出窗口
    /// </summary>
    /// <param name="itemDefinition">要确认的物品定义</param>
    private void OpenConfirmPopup(ItemDefinitionSO itemDefinition) {
        _pendingItem = itemDefinition;
        FirstSelectedButton = confirmButton;
        confirmPopup.gameObject.SetActive(true);

        popupText.text = $"{itemDefinition.ItemName} 成功率：{itemDefinition.RarityWeight}%";

        SetButtonInteractable(false);
        ReBindButtons(confirmButton, OnConfirm);
        ReBindButtons(cancelButton, ClosePopup);

        SetDefaultSelection(); 
    }

    /// <summary>
    /// 清除物品列表，销毁所有物品按钮，并清空按钮列表。确保在面板关闭或重新打开时，旧的物品按钮不会残留，避免界面混乱和内存泄漏。
    /// </summary>
    private void ClearItemList() {
        foreach(StealItemButton stealItemButton in _stealItemButtons) {
            Destroy(stealItemButton.gameObject);
        }

        _stealItemButtons.Clear();

        FirstSelectedButton = null;
    }

    /// <summary>
    /// 设置物品按钮的可交互状态
    /// </summary>
    /// <param name="interactable">是否可交互</param>
    private void SetButtonInteractable(bool interactable) {
        foreach (StealItemButton button in _stealItemButtons) {
            button.CurrentButton.interactable = interactable;
        }
    }

    private void HidePopup() {
        _pendingItem = null;
        confirmPopup.gameObject.SetActive(false);
        SetButtonInteractable(true);
    }

    private void ClosePopup() {
        HidePopup();
        RefreshItemList();
    }

    public override bool HandleCancelInput() {
        if(confirmPopup.gameObject.activeSelf) {
            ClosePopup();
            return true;
        }

        return false;
    }
}
