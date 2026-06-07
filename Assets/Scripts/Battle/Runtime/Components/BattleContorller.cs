using Framework.Event;

/// <summary>
/// 战斗部分的核心逻辑控制器，负责管理战斗状态、执行战斗循环、维护战斗实体列表等核心功能。
/// </summary>
public class BattleContorller : MonoBehaviour,
    IEventReceiver<GameModeChangedEvent> {
    [SerializeField] private BattleFieldManager fieldManager;

    public BattleFieldManager FieldManager => fieldManager;

    private readonly List<BattleEntity> _allEntities = new();
    public List<BattleEntity> AllEntities => _allEntities;

    public BattleEntity CurrentEntity { get; set; }

    private BattleState _currentState; // 当前战斗状态

    private bool _battleRunning;

    public bool IsBattleRunning => _battleRunning;

    private Coroutine _battleLoopRoutine; // 当前正在运行的战斗循环协程

    private void OnEnable() {
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }

    #region 事件响应
    public void OnEvent(GameModeChangedEvent evt) {
        if(evt.NewGameMode == GameMode.Battle) {
            StartBattleIfReady();
            return;
        }

        if(_battleRunning) {
            StopBattle();
        }
    }
    #endregion

    public void SetState(BattleState nextState) => _currentState = nextState;

    /// <summary>
    /// 检查是否满足进入战斗的条件，如果满足则启动战斗循环。通常在游戏模式切换到战斗模式时调用。
    /// </summary>
    private void StartBattleIfReady() {
        if (_battleRunning) return;
        if (GameModeManager.Instance.CurrentGameMode != GameMode.Battle) return;
        if (!BattleService.Instance.HasPendingPreload) return;

        BattleStartPreload preload = BattleService.Instance.ConsumeStartPreload();

        // 进入战斗第一个状态，通常是战斗准备状态
        SetState(new BattleSetupState(this, preload));

        _battleRunning = true;
        _battleLoopRoutine = StartCoroutine(BattleLoopRoutine());

    }


    /// <summary>
    /// 标准状态机的战斗循环，持续执行当前状态的Enter、Execute、Exit方法，直到战斗结束或状态发生变化。
    /// </summary>
    /// <returns></returns>
    private IEnumerator BattleLoopRoutine() {
        while (_battleRunning && _currentState != null) {
            // 存储当前状态的快照，以防在执行过程中状态发生变化
            BattleState stateSnapshot = _currentState;

            yield return StartCoroutine(stateSnapshot.Enter());

            // 在执行过程中，如果状态发生变化，立即跳出当前状态的执行，进入新的状态
            if (stateSnapshot != _currentState) {
                yield return StartCoroutine(stateSnapshot.Exit());
                continue;
            }

            yield return StartCoroutine(stateSnapshot.Execute());

            yield return StartCoroutine(stateSnapshot.Exit());

        }

        _battleLoopRoutine = null;
    }

    /// <summary>
    /// 停止当前战斗并触发战斗结束流程，清理相关状态与资源。
    /// </summary>
    private void StopBattle() {
        _battleRunning = false;
        _currentState = null;
        if(_battleLoopRoutine != null) {
            StopCoroutine(_battleLoopRoutine);
            _battleLoopRoutine = null;
        }
    }
}
