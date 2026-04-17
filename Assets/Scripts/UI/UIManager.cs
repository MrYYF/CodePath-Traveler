using Framework.Event;
using System;


public class UIManager : MonoBehaviour, 
    IEventReceiver<PanelRequestEvent> {

    [Header("根节点与特殊面板引用")]
    [SerializeField,Tooltip("探索模式下显示总体 UI 根节点")] 
    private GameObject fieldUIRoot;

    private readonly Dictionary<Type,PanelController> _panelControllerDict = new();
    private readonly List<PanelController> _allPanelList = new();

    #region 生命周期函数
    private void Awake() {
        _panelControllerDict.Clear();
        _allPanelList.Clear();

        GetPanelsFromRoot(transform);
    }
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

    private void GetPanelsFromRoot(Transform root) {
        var panels = root.GetComponentsInChildren<PanelController>(true);
        foreach (var panel in panels) {
            _allPanelList.Add(panel);
            if (panel.PanelActionType == null) 
                return;
            _panelControllerDict.Add(panel.PanelActionType, panel);
        }
    }

    private void TryHandleCancelByActivePanel() {
        foreach (var panel in _allPanelList) {
            if (panel.gameObject.activeSelf) {
                panel.gameObject.SetActive(false);
                return;
            }
        }
    }
    private bool IsAnyPanelOpen() {
        foreach (var panel in _allPanelList) {
            if (panel.gameObject.activeSelf) {
                return true;
            }
        } 
        return false;
    }

    private void CloseAllPanel() {
        foreach (var panel in _allPanelList) {
            panel.gameObject.SetActive(false);
        }
    }

    #region 事件相关
    public void OnEvent(PanelRequestEvent evt) {
        var panelType = evt.actionBase.GetType();
        _panelControllerDict.TryGetValue(panelType, out var panel);

        panel?.SetupPanel(evt.actionBase);
        panel?.gameObject.SetActive(true);
    }
    #endregion
}
