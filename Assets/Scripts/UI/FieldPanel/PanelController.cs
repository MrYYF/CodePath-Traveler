using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelController : MonoBehaviour {
    [Header("Action")]
    public ActionBase CurrentAction;

    [Header("Focus Navigation")]
    public Button FirstSelectedButton; // 首选按钮，面板打开时自动选中

    [Header("Action icon")]
    public Image ActionIcon; // 显示当前Action的图标

    public virtual Type PanelActionType => null;

    public virtual void SetupPanel(ActionBase action) {
        CurrentAction = action;
        if (ActionIcon != null)
            ActionIcon.sprite = action.CommandInfo.Icon;
        if (FirstSelectedButton != null)
            SetDefaultSelection();

    }

    public virtual void ClosePanel() {
        gameObject.SetActive(false);
    }

    protected virtual void OnCancel() {
        GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore);
        ClosePanel();
    }

    protected virtual void OnConfirm() {
        CurrentAction.Execute();
        ClosePanel();
    }

    public virtual bool HandleCancelInput() {
        return false;
    }

    // 设置默认选中按钮，确保在面板打开时有一个按钮被选中，方便使用键盘或手柄导航
    public void SetDefaultSelection() {
        FirstSelectedButton.Select();
        EventSystem.current.SetSelectedGameObject(FirstSelectedButton.gameObject);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    protected void ReBindButtons(Button button, UnityAction unityAction) {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(unityAction);
    }
}