
using Framework.Event;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

/// <summary>
/// 交互UI控制器，负责监听交互状态变化事件，并根据当前交互对象的可用指令列表动态显示对应的头顶图标
/// </summary>
public class InteractionUIController : MonoBehaviour, 
    IEventReceiver<InteractionChangedEvent>,
    IEventReceiver<InteractionMenuRequestEvent>,
    IEventReceiver<GameModeChangedEvent> {

    [Header("Head Icon")]
    [SerializeField] private RectTransform actionIconHolder;
    [SerializeField] private GameObject actionIconPrefab;
    private ObjectPool<GameObject> _iconPool;
    private readonly List<GameObject> _activeIcons = new(8);


    [Header("Menu Button")]
    [SerializeField] private RectTransform actionMenuHolder;
    [SerializeField] private GameObject actionMenuButtonPrefab;
    private ObjectPool<GameObject> _menuButtonPool;
    private readonly List<GameObject> _activeButtons = new(8);

    private IReadOnlyList<ActionCommandInfo> _currentCommandList; // 当前显示的指令列表
    private Transform _headAnchor;
    private InteractionBase _target;

    #region Unity生命周期
    private void Awake() {
        InitPool();
        actionIconHolder.gameObject.SetActive(false); // 初始状态隐藏图标容器
        actionMenuHolder.gameObject.SetActive(false); // 初始状态隐藏容器
    }
    private void OnEnable() {
        EventBus.Subscribe<InteractionChangedEvent>(this);
        EventBus.Subscribe<InteractionMenuRequestEvent>(this);
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<InteractionChangedEvent>(this);
        EventBus.Unsubscribe<InteractionMenuRequestEvent>(this);
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }
    private void Update() {
        if(GameModeManager.Inastance.CurrentGameMode != GameMode.InteractionMenu)
            return;

        var input = InputSystemController.Inastance;
        if (input.GetUICancelPressed()) {
            //TODO: 如果在Action二级面板打开时按下会有BUG
            CloseMenu(true);
            GameModeManager.Inastance.RequestChangeGameMode(GameMode.Explore);
        }

    }
    private void LateUpdate() {
        UpdateHeadIconPosition();
    }

    #endregion

    #region 对象池相关方法
    private void InitPool() {
        _iconPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(actionIconPrefab, actionIconHolder),
            actionOnGet: icon => { 
                icon.SetActive(true);
                icon.transform.SetAsLastSibling(); // 确保新获取的图标在最后面
            },
            actionOnRelease: icon => icon.SetActive(false),
            actionOnDestroy: icon => Destroy(icon),
            collectionCheck: false,
            defaultCapacity: 8,
            maxSize: 16
        );

        _menuButtonPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(actionMenuButtonPrefab, actionMenuHolder),
            actionOnGet: button => { 
                button.SetActive(true);
                button.transform.SetAsLastSibling(); // 确保新获取的按钮在最后面
            },
            actionOnRelease: button => button.SetActive(false),
            actionOnDestroy: button => Destroy(button),
            collectionCheck: false,
            defaultCapacity: 8,
            maxSize: 16
        );
    }
    

    private void syncPool(List<GameObject> activeList, ObjectPool<GameObject> pool, int targetCount) {
        while (activeList.Count > targetCount) {
            var icon = activeList[activeList.Count - 1];
            activeList.RemoveAt(activeList.Count - 1);
            pool.Release(icon);
        }

        while (activeList.Count < targetCount) {
            var icon = pool.Get();
            activeList.Add(icon);
        }
    }

    private void ReleaseAllPool(List<GameObject> activeList, ObjectPool<GameObject> pool) {
        for (int i = activeList.Count - 1; i >= 0; i--) {
            pool.Release(activeList[i]);
        }
        activeList.Clear();
    }

    #endregion

    #region 事件相关方法

    // 启动头顶icon
    public void OnEvent(InteractionChangedEvent evt) {
        _target = evt.target;
        if (!evt.inRange || _target == null) {
            HideHeadIcons();
            return;
        }

        _currentCommandList = _target.CachedCommandInfo;
        _headAnchor = _target.HeadAnchor;
        ShowHeadIcons();
    }

    // 启动指令菜单
    public void OnEvent(InteractionMenuRequestEvent evt) {
        if (evt.target == null)
            return;

        HideHeadIcons();
        actionMenuHolder.gameObject.SetActive(true);

        OpenMenu(evt.target);
    }

    public void OnEvent(GameModeChangedEvent evt) {
        if(evt.NewGameMode == GameMode.InteractionMenu) 
            return;
        

        if(evt.NewGameMode == GameMode.Explore) {
            Debug.Log("切换回探索模式，恢复头顶图标显示");
            ShowHeadIcons();
        }
    }
    #endregion

    private void ShowHeadIcons() {
        if (_currentCommandList is null || _currentCommandList.Count <= 0)
            return;

        actionIconHolder.gameObject.SetActive(true);
        syncPool(_activeIcons, _iconPool, _currentCommandList.Count);

        for (int i = 0; i < _currentCommandList.Count; i++) {
            var icon = _activeIcons[i];
            var commandInfo = _currentCommandList[i];

            icon.GetComponent<Image>().sprite = commandInfo.Icon;
        }
    }

    // 更新头顶图标的位置，使其始终跟随交互对象
    private void UpdateHeadIconPosition() {
        if(_target == null || !_target.isActiveAndEnabled) {
            HideHeadIcons();
            return;
        }
        if (_headAnchor == null || !actionIconHolder.gameObject.activeSelf || _activeIcons.Count == 0)
            return;
        var worldPos = _headAnchor.position;
        var screenPos = Camera.main.WorldToScreenPoint(_headAnchor.position);

        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, 
            screenPos, 
            null, 
            out var localPos)) {
            actionIconHolder.anchoredPosition = localPos;
        }
    }

    private void OpenMenu(InteractionBase target) {
        GameModeManager.Inastance.RequestChangeGameMode(GameMode.InteractionMenu);

        syncPool(_activeButtons, _menuButtonPool, _currentCommandList.Count);

        Button firstButton = null;
        // 按钮绑定方法
        for (int i = 0; i < _activeButtons.Count; i++) {
            var button = _activeButtons[i].GetComponent<ActionMenuButton>();
            var cmd = _currentCommandList[i];
            int index = i; // 避免闭包问题
            button.SetButton(cmd, () => {
                target.ExecuteCommandFromUI(index);
                CloseMenu(false);
            });

            if (firstButton == null) firstButton = button.GetComponent<Button>();
        }

        if (firstButton != null) {
            firstButton.Select();
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    private void CloseMenu(bool restoreHeadIcons) {
        HideAcitonMenu();
        if(restoreHeadIcons) {
            ShowHeadIcons();
        }
        else {
            HideHeadIcons();
        }
    }

    private void HideHeadIcons() {
        actionIconHolder?.gameObject.SetActive(false);
        ReleaseAllPool(_activeIcons, _iconPool);
    }

    private void HideAcitonMenu() {
        actionMenuHolder?.gameObject.SetActive(false);
        ReleaseAllPool(_activeButtons, _menuButtonPool);
    }

    
}
