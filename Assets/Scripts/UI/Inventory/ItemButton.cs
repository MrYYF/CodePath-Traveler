
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// 物品按钮基类，负责显示物品信息和处理选中状态
/// </summary>
public class ItemButton : MonoBehaviour, ISelectHandler, IDeselectHandler {

    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemDescription;
    [SerializeField] private TMP_Text itemQuantity;
    [SerializeField] private GameObject itemTips;

    private Button _button;
    public Button CurrentButton => _button;

    protected InventoryItem _currentItem;
    protected ItemDefinitionSO CurrentItemDefinition => _currentItem?.ItemDefinition;

    private Action<ItemDefinitionSO> _onItemClick;

    protected virtual void Awake() {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    protected virtual void OnClick() {

    }

    protected virtual void SetupButton(InventoryItem inventoryItem) => SetupButton(inventoryItem, null);

    protected virtual void SetupButton(InventoryItem inventoryItem, Action<ItemDefinitionSO> onItemClick) {
        _currentItem = inventoryItem;
        _onItemClick = onItemClick;

        itemIcon.sprite = InventoryManager.Inastance.IconSet.GetIconForItem(inventoryItem.ItemDefinition.itemIconKey);
        itemName.text = inventoryItem.ItemDefinition.ItemName;
        itemDescription.text = inventoryItem.ItemDefinition.ItemDescription;
        if(itemQuantity != null)
            itemQuantity.text = inventoryItem.Quantity.ToString();


    }

    #region UI回调
    public void OnDeselect(BaseEventData eventData) {
        itemTips.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData) {
        itemTips.SetActive(true);
    }
    #endregion
}
