using System;
using TMPro;
using UnityEngine.UI;

public class InquirePanelController : PanelController
{
    [Header("Inquire Panel")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image avatar;
    [SerializeField] private TMP_Text messageTitleText;
    [SerializeField] private TMP_Text messageContentText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;

    private InquireAction _currentAction;
    private int _currentIndex = -1;

    public override Type PanelActionType => typeof(InquireAction);

    public override void SetupPanel(ActionBase action) {
        base.SetupPanel(action);
        FirstSelectedButton = confirmButton;

        ReBindButtons(confirmButton, OnCancel);
        SetDefaultSelection();

        _currentAction = action as InquireAction;

        ApplyMessage(_currentAction.PickRandomMessageIndex());
    }

    private void ApplyMessage(int messageIndex) {
        _currentAction.GetInquireActionData(messageIndex, out var data);
        _currentIndex = messageIndex;

        nameText.text = data.personName;
        avatar.sprite = data.portraitOverride;
        messageTitleText.text = data.title;
        messageContentText.text = data.message;
    }
}
