using System;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 招募面板控制器，负责显示NPC的基本信息，并处理玩家的确认或取消操作
/// </summary>
public class RecruitPanelController : PanelController
{
    [Header("Recruit Panel")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public override Type PanelActionType => typeof(RecruitAction);

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);
        RecruitAction recruitAction = action as RecruitAction;

        levelText.text = recruitAction.CurrentCharacter.BaseLevel.ToString();
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
