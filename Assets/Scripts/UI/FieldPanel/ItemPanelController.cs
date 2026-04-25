


using System;

public class ItemPanelController : PanelController
{
    [Header("Item Panel")]
    [SerializeField] private ItemButton itemButtonPrefab; // 物品按钮预制体
    [SerializeField] private RectTransform itemButtonParent; // 物品按钮的父对象

    private readonly List<ItemButton> itemButtons = new();
    private PanelType _currentPanelType;
    private Action<ItemDefinitionSO> _onItemClick;

    public void SetupPanel(PanelType panelType, ActionBase actionBase = null, Action<ItemDefinitionSO> onItemClick = null) {
        if(actionBase == null)
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
                break;
            default:
                break;
        }
    }

    private void ClearItemListView() {
        foreach (ItemButton button in itemButtons) {
            Destroy(button.gameObject);
        }
        itemButtons.Clear();
        FirstSelectedButton = null;

    }

    private void BuildBuyList(InventoryManager inventoryManager) {

    }

}
