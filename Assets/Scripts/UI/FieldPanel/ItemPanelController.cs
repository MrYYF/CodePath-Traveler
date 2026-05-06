using System;

public class ItemPanelController : PanelController
{
    [Header("Item Panel")]
    [SerializeField] private ItemButton itemButtonPrefab; // 物品按钮预制体
    [SerializeField] private RectTransform itemButtonParent; // 物品按钮的父对象

    private readonly List<ItemButton> _itemButtons = new();
    private PanelType _currentPanelType;
    private Action<ItemDefinitionSO> _onItemClick;

    /// <summary>
    /// 用于设置面板内容的方法，根据传入的面板类型和当前正在执行的动作来构建物品列表，并设置物品点击回调函数。
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <param name="actionBase">正在执行的动作</param>
    /// <param name="onItemClick">物品点击回调函数</param>
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
            case PanelType.Item:
                BuildItemList(inventoryManager);
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

    /// <summary>
    /// 清空物品列表视图的方法，销毁所有现有的物品按钮并清空按钮列表，同时将默认选中按钮设置为null。
    /// </summary>
    private void ClearItemListView() {
        foreach (ItemButton button in _itemButtons) {
            Destroy(button.gameObject);
        }
        _itemButtons.Clear();
        FirstSelectedButton = null;

    }

    /// <summary>
    /// 构建购买列表的方法，根据当前正在执行的ShopAction中的物品列表来创建物品按钮，并获取玩家当前拥有的数量以显示或限制购买数量。
    /// </summary>
    /// <param name="inventoryManager">库存管理器实例</param>
    private void BuildBuyList(InventoryManager inventoryManager) {
        ShopAction shopAction = (ShopAction)CurrentAction;
        foreach (InventoryItem item in shopAction.itemsBag) {
            int playerQuantity = inventoryManager.GetItemQuantity(item.ItemDefinition); // 获取玩家当前拥有的数量，可能用于显示或限制购买数量
            AddItemButton(new InventoryItem(item.ItemDefinition, playerQuantity));
        }

    }

    /// <summary>
    /// 构建出售列表的方法，根据玩家当前库存中的物品来创建物品按钮，允许玩家选择要出售的物品和数量。
    /// </summary>
    /// <param name="inventoryManager">库存管理器实例</param>
    private void BuildSellList(InventoryManager inventoryManager) {
        foreach (InventoryItem item in inventoryManager.CurrentInventory) {
            AddItemButton(new InventoryItem(item.ItemDefinition, item.Quantity));
        }

    }

    private void BuildItemList(InventoryManager inventoryManager) {
        foreach (InventoryItem item in inventoryManager.CurrentInventory) {
            AddItemButton(item);
        }
    }

    /// <summary>
    /// 添加物品按钮的方法，根据传入的InventoryItem创建一个新的ItemButton实例，并设置其交互状态和显示格式，然后将其添加到按钮列表中以供后续管理和显示。
    /// </summary>
    /// <param name="inventoryItem">要添加的物品项</param>
    /// <param name="interactable">按钮是否可交互</param>
    /// <param name="equippedNameFormat">是否使用装备名称格式</param>
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

    /// <summary>
    /// 刷新物品数量的方法，根据传入的物品定义找到对应的物品按钮，并获取玩家当前拥有的数量来更新按钮上的显示数量，以确保界面上的信息与玩家实际库存保持一致。
    /// </summary>
    /// <param name="itemDefinition">要刷新的物品定义</param>
    public void RefreshItemQuantity(ItemDefinitionSO itemDefinition) {
        var itemButton = _itemButtons.Find(button => button.CurrentItemDefinition == itemDefinition);
        if (itemButton == null) return;
        int playerQuantity = InventoryManager.Instance.GetItemQuantity(itemDefinition);
        itemButton.UpdateQuantity(playerQuantity);
    }

    /// <summary>
    /// 删除物品按钮的方法，根据传入的物品定义找到对应的物品按钮，并将其从按钮列表中移除并销毁，以更新界面上的物品列表显示，确保玩家无法再选择已删除的物品。
    /// </summary>
    /// <param name="itemDefinition">要删除的物品定义</param>
    internal void RemoveItemButton(ItemDefinitionSO itemDefinition) {
        var itemButtonIndex = _itemButtons.FindIndex(button => button.CurrentItemDefinition == itemDefinition);
        ItemButton itemButton = _itemButtons[itemButtonIndex];
        _itemButtons.RemoveAt(itemButtonIndex);
        Destroy(itemButton.gameObject);

        FirstSelectedButton = _itemButtons.Count > 0 ? _itemButtons[0].CurrentButton : null;
        if (_itemButtons.Count > 0) {
            int nextIndex = Mathf.Min(itemButtonIndex, _itemButtons.Count - 1);
            FirstSelectedButton = _itemButtons[nextIndex].CurrentButton;
            SetDefaultSelection();
        }


    }
}
