
using System;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 挑战面板控制器，负责显示挑战NPC的基本信息，并处理玩家的确认或取消操作
/// </summary>
public class ChallengePanelController : PanelController
{
    [Header("Challenge Panel")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public override Type PanelActionType => typeof(ChallengeAction);

    private void Awake() {
        //ReBindButtons(confirmButton, OnConfirm);
        
    }

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);
        ChallengeAction challengeAction = action as ChallengeAction;

        npcNameText.text = challengeAction.CurrentCharacter.Name;
        difficultyText.text = $"旗鼓相当的对手";
        characterImage.sprite = challengeAction.CurrentCharacter.Portrait;

        ReBindButtons(cancelButton, OnCancel);
        FirstSelectedButton = confirmButton;
        SetDefaultSelection();
    }
}
