using Framework.Event;
using System;


public class UIManager : MonoBehaviour, 
    IEventReceiver<PanelRequestEvent> {

    [Header("根节点与特殊面板引用")]
    [SerializeField,Tooltip("探索模式下显示总体 UI 根节点")] 
    private GameObject fieldUIRoot;
    [SerializeField] private GameObject mainPanel;

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
        var mode = GameModeManager.Instance.CurrentGameMode;
        var input = InputSystemController.Instance;
        if (mode == GameMode.Battle) return;
        if (input.GetUICancelPressed()) {
            HandleGlobalCancelInput(mode);
            return;
        }
        if(input.GetMenuPressed()) {
            HandleGlobalMenuInput(mode);
        }
    }
    #endregion


    /// <summary>
    /// 处理全局输入菜单键逻辑
    /// </summary>
    /// <param name="currentMode">当前的游戏模式</param>
    private void HandleGlobalMenuInput(GameMode currentMode) {
        // 如果主菜单面板已经打开，则关闭主菜单面板，并切换回探索模式
        if (mainPanel.activeInHierarchy) {
            CloseAllPanel();
            mainPanel.SetActive(false);
            GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore);
            return;
        }

        // 在战斗模式和交互菜单模式下，菜单键不应该打开主菜单面板，因此直接返回，不执行后续逻辑
        if (currentMode == GameMode.Battle || currentMode == GameMode.InteractionMenu) 
            return;

        // 如果当前有任何面板打开，则不处理菜单输入，避免与面板内的菜单键冲突
        if (IsAnyPanelOpen()) 
            return;

        mainPanel.SetActive(true);
        GameModeManager.Instance.RequestChangeGameMode(GameMode.Pause);
    }

    /// <summary>
    /// 处理全局输入取消键逻辑
    /// </summary>
    /// <param name="currentMode">当前的游戏模式</param>
    private void HandleGlobalCancelInput(GameMode currentMode) {
        // 尝试通过当前打开的面板处理取消输入，如果面板处理了取消输入，则不执行后续逻辑
        if (TryHandleCancelByActivePanel())
            return;

        // 如果没有任何面板打开，则不执行后续逻辑
        if (!IsAnyPanelOpen()) {
            if(currentMode != GameMode.Explore)
                // 切换回探索模式
                GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore);
            return;
        }

        // 关闭所有面板
        CloseAllPanel();
        GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore);
    }


    private void GetPanelsFromRoot(Transform root) {
        var panels = root.GetComponentsInChildren<PanelController>(true);
        foreach (var panel in panels) {
            _allPanelList.Add(panel);
            if (panel.PanelActionType == null)
                continue;
            _panelControllerDict.Add(panel.PanelActionType, panel);
        }
    }

    private bool TryHandleCancelByActivePanel() {
        foreach (var panel in _allPanelList) {
            if (panel.gameObject.activeSelf == false) {
                continue;
            }
            if(panel.HandleCancelInput()) {
                return true;
            }
        }
        return false;
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

        panel?.gameObject.SetActive(true);
        panel?.SetupPanel(evt.actionBase);
    }
    #endregion
}
