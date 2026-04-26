


using System;

public class ItemPanelController : PanelController
{
    [Header("Item Panel")]
    [SerializeField] private ItemButton itemButtonPrefab; // 物品按钮预制体
    [SerializeField] private RectTransform itemButtonParent; // 物品按钮的父对象

    private readonly List<ItemButton> _itemButtons = new();
    private PanelType _currentPanelType;
    private Action<ItemDefinitionSO> _onItemClick;

    public void SetupPanel(PanelType panelType, ActionBase actionBase = null, Action<ItemDefinitionSO> onItemClick = null) {
        if (actionBase != null)
            base.SetupPanel(actionBase);

        _currentPanelType = panelType;
        _onItemClick = onItemClick;

        ClearItemListView();

        var inventoryManager = InventoryManager.Instance;
        switch (panelType) {
            case PanelType.Buy:
                BuildBuyList(inventoryManager);
                break;
            case PanelType.Sell:
                BuildSellList(inventoryManager);
                break;
            default:
                break;
        }

        if (_itemButtons.Count == 0) {
            FirstSelectedButton = null;
            return;
        }

        FirstSelectedButton = _itemButtons[0].CurrentButton;
        SetDefaultSelection();
    }

    private void ClearItemListView() {
        foreach (ItemButton button in _itemButtons) {
            Destroy(button.gameObject);
        }
        _itemButtons.Clear();
        FirstSelectedButton = null;

    }

    private void BuildBuyList(InventoryManager inventoryManager) {
        ShopAction shopAction = (ShopAction)CurrentAction;
        foreach (InventoryItem item in shopAction.itemsBag) {
            int playerQuantity = inventoryManager.GetItemQuantity(item.ItemDefinition); // 获取玩家当前拥有的数量，可能用于显示或限制购买数量
            AddItemButton(new InventoryItem(item.ItemDefinition, playerQuantity));
        }

    }
    private void BuildSellList(InventoryManager inventoryManager) {
        foreach (InventoryItem item in inventoryManager.CurrentInventory) {
            AddItemButton(new InventoryItem(item.ItemDefinition, item.Quantity));
        }

    }

    private void AddItemButton(InventoryItem inventoryItem, bool interactable = true, bool equippedNameFormat = false) {
        ItemButton itemButton = Instantiate(itemButtonPrefab, itemButtonParent);
        if (itemButton is ShopItemButton shopItemButton) {
            shopItemButton.SetupButton(inventoryItem, _currentPanelType, _onItemClick);
        }
        else {
            itemButton.SetupButton(inventoryItem, _onItemClick);
        }

        itemButton.CurrentButton.interactable = interactable;
        _itemButtons.Add(itemButton);
    }
}
