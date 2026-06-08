using Framework.Event;

/// <summary>
/// 
/// </summary>
public class HealthBarController : MonoBehaviour,
    IEventReceiver<BattleStartedEvent> {

    [SerializeField] private HealthBar healthBarPrefab;

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<BattleStartedEvent>(this);
        RebuildHealthBars();
    }
    private void OnDisable() {
        EventBus.Unsubscribe<BattleStartedEvent>(this);
    }
    #endregion

    private void RebuildHealthBars() {
        // 清空旧状态栏
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        BattleUnit[] allUnits = FindObjectsOfType<BattleUnit>();
        for (int i = 0; i < allUnits.Length; i++) {
            BattleUnit unit = allUnits[i];
            if(unit.Entity == null || !unit.Entity.IsPlayer) {
                continue;
            }

            HealthBar healthBar = Instantiate(healthBarPrefab, transform);
            healthBar.Setup(unit.Entity);
        }
    }


    #region 事件监听
    public void OnEvent(BattleStartedEvent evt) {
        RebuildHealthBars();
    }

    #endregion
}
