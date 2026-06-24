
using Framework.Event;

public class WeaknessController : MonoBehaviour,
    IEventReceiver<BattleStartedEvent>,
    IEventReceiver<BattleEndedEvent> {
    [Header("Prefab")]
    [SerializeField] private WeaknessBar weaknessBarPrefab;
    [SerializeField] private DamageTypeIconSetSO damageTypeIconSet;

    [Header("Follow")]
    [SerializeField] private RectTransform containerRoot;
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    private readonly Dictionary<BattleEntity, WeaknessBar> _barByEntity = new();

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<BattleStartedEvent>(this);
        EventBus.Subscribe<BattleEndedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<BattleStartedEvent>(this);
        EventBus.Unsubscribe<BattleEndedEvent>(this);
        ClearBra();
    }
    private void LateUpdate() {
        if (_barByEntity.Count == 0) {
            return;
        }

        foreach (var kv in _barByEntity) {
            BattleEntity entity = kv.Key;
            WeaknessBar bar = kv.Value;

            if (!entity.IsAlive) {
                bar.SetVisible(false);
                continue;
            }

            // 屏幕坐标转世界坐标
            Vector3 screenPos = Camera.main.WorldToScreenPoint(entity.Unit.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRoot, screenPos, null, out Vector2 localPos);
            bar.SetVisible(true);
            bar.SetScreenPosition(localPos + screenOffset);

        }
    }
    #endregion

    private void RebuildBars() {
        ClearBra();
        BattleUnit[] allUnits = FindObjectsOfType<BattleUnit>(true);

        foreach (var unit in allUnits) {
            if (unit.Entity.IsPlayer) {
                continue;
            }

            WeaknessBar bar = Instantiate(weaknessBarPrefab, containerRoot);
            bar.Setup(unit.Entity, damageTypeIconSet);

            _barByEntity[unit.Entity] = bar;
        }
    }

    /// <summary>
    /// 清除状态栏
    /// </summary>
    private void ClearBra() {
        foreach (var bar in _barByEntity.Values) {
            Destroy(bar.gameObject);
        }
        _barByEntity.Clear();
    }

    #region 事件监听
    public void OnEvent(BattleStartedEvent evt) {
        RebuildBars();
    }

    public void OnEvent(BattleEndedEvent evt) {
        ClearBra();
    }
    #endregion
}
