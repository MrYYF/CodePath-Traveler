
using Framework.Event;
using UnityEngine.Pool;
using UnityEngine.UI;

/// <summary>
/// 交互UI控制器，负责监听交互状态变化事件，并根据当前交互对象的可用指令列表动态显示对应的头顶图标
/// </summary>
public class InteractionUIController : MonoBehaviour, IEventReceiver<InteractionChangedEvent> {
    [Header("Head Icon")]
    [SerializeField] private RectTransform actionIconHolder;
    [SerializeField] private GameObject actionIconPrefab;

    private ObjectPool<GameObject> _iconPool;
    private readonly List<GameObject> _activeIcons = new(8);
    private IReadOnlyList<ActionCommandInfo> _currentCommandList; // 当前显示的指令列表

    #region Unity生命周期
    private void Awake() {
        InitPool();

        actionIconHolder.gameObject.SetActive(false); // 初始状态隐藏图标容器
    }
    private void OnEnable() {
        EventBus.Subscribe<InteractionChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<InteractionChangedEvent>(this);
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

    #endregion

    #region 事件相关方法
    public void OnEvent(InteractionChangedEvent evt) {
        _currentCommandList = evt.target.CachedCommandInfo;

        ShowHeadIcons();
    }
    #endregion

    private void ShowHeadIcons() {
        actionIconHolder.gameObject.SetActive(_currentCommandList.Count > 0);
        syncPool(_activeIcons, _iconPool, _currentCommandList.Count);

        for (int i = 0; i < _currentCommandList.Count; i++) {
            var icon = _activeIcons[i];
            var commandInfo = _currentCommandList[i];

            icon.GetComponent<Image>().sprite = commandInfo.Icon;
        }
    }
}
