


using TMPro;
using UnityEngine.UI;

public class RecruitPanelController : PanelController
{
    [Header("Recruit Panel")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);
        RecruitAction recruitAction = action as RecruitAction;

        npcNameText.text = recruitAction.CurrentCharacter.Name;
        characterImage.sprite = recruitAction.CurrentCharacter.Portrait;

        BindButtons();
        SetDefaultSelection();
    }

    private void BindButtons() {
        ReBindButtons(confirmButton, OnConfirm);
        ReBindButtons(cancelButton, OnCancel);
    }
}
