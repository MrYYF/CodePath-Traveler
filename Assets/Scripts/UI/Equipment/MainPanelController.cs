

using System;
using TMPro;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour
{
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
        itemButton.onClick.AddListener(OpenItemPanel);
        equipmentButton.onClick.AddListener(OpenEquipmentPanel);
    }

    private void OpenItemPanel() {
        OpenPanel(itemPanelController.gameObject, itemButton);
        itemPanelController.SetupPanel(PanelType.Item);
    }

    private void OpenEquipmentPanel() {
        OpenPanel(equipmentPanelController.gameObject, equipmentButton);
    }

    private void OpenPanel(GameObject panel, Button button) {
        _currentOpenPanel = panel;
        _currentSelectedButton = button;

        panel.SetActive(true);
        leftPartCanvasGroup.interactable = false;


    }
}
