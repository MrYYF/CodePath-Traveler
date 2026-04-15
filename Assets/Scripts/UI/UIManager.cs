using Framework.Event;


public class UIManager : MonoBehaviour, 
    IEventReceiver<PanelRequestEvent> {

    [Header("根节点与特殊面板引用")]
    [SerializeField,Tooltip("探索模式下显示总体 UI 根节点")] 
    private GameObject fieldUIRoot;

    public InquirePanelController inquirePanelController;

    #region 生命周期函数
    private void OnEnable() {
        EventBus.Subscribe<PanelRequestEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<PanelRequestEvent>(this);
    }
    #endregion

    public void OnEvent(PanelRequestEvent evt) {
        if(evt.actionBase is InquireAction) {
            inquirePanelController.gameObject.SetActive(true);
            inquirePanelController.SetupPanel(evt.actionBase);
        }
    }
}
