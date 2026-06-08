using Framework.Event;

/// <summary>
/// 战斗HUD控制器
/// </summary>
public class BattleHUDController : MonoBehaviour,
    IEventReceiver<BattleStartedEvent>,
    IEventReceiver<GameModeChangedEvent> {

    [Header("HUD Panels")]
    [SerializeField] private GameObject ctbPanel;
    [SerializeField] private GameObject healthBarPanel;

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<BattleStartedEvent>(this);
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<BattleStartedEvent>(this);
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }
    #endregion

    private void SetHUDVisible(bool visible) {
        ctbPanel.SetActive(visible);
        healthBarPanel.SetActive(visible);
    }


    #region 事件监听
    public void OnEvent(BattleStartedEvent evt) {
        SetHUDVisible(true);
    }

    public void OnEvent(GameModeChangedEvent evt) {
        SetHUDVisible(false);
    }

    #endregion
}
