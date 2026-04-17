using Framework.Event;


public class UIManager : MonoBehaviour, 
    IEventReceiver<PanelRequestEvent> {

    [Header("根节点与特殊面板引用")]
    [SerializeField,Tooltip("探索模式下显示总体 UI 根节点")] 
    private GameObject fieldUIRoot;

    public InquirePanelController inquirePanelController;
    public RecruitPanelController recruitPanelController;

    #region 生命周期函数
    private void OnEnable() {
        EventBus.Subscribe<PanelRequestEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<PanelRequestEvent>(this);
    }
    private void Update() {
        var mode = GameModeManager.Inastance.CurrentGameMode;
        if (mode == GameMode.Battle) return;
        if(mode == GameMode.InteractionMenu) {
            if(IsAnyPanelOpen() && InputSystemController.Inastance.GetUICancelPressed()) {
                TryHandleCancelByActivePanel();
                return;
            };
        }

        if(InputSystemController.Inastance.GetUICancelPressed()) {
            CloseAllPanel();
        }
    }
    #endregion

    private void TryHandleCancelByActivePanel() {
        if(inquirePanelController.gameObject.activeSelf)
            inquirePanelController.ClosePanel();
        if(recruitPanelController.gameObject.activeSelf)
            recruitPanelController.ClosePanel();
    }
    private bool IsAnyPanelOpen() {
        return inquirePanelController.gameObject.activeSelf || recruitPanelController.gameObject.activeSelf;
    }

    private void CloseAllPanel() {
        inquirePanelController.gameObject.SetActive(false);
        recruitPanelController.gameObject.SetActive(false);
    }

    #region 事件相关
    public void OnEvent(PanelRequestEvent evt) {
        if(evt.actionBase is InquireAction) {
            inquirePanelController.gameObject.SetActive(true);
            inquirePanelController.SetupPanel(evt.actionBase);
        }

        if(evt.actionBase is RecruitAction) {
            recruitPanelController.gameObject.SetActive(true);
            recruitPanelController.SetupPanel(evt.actionBase);
        }
    }
    #endregion
}
