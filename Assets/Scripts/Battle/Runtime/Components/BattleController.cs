using Framework.Event;

/// <summary>
/// 战斗部分的核心逻辑控制器，负责管理战斗状态、执行战斗循环、维护战斗实体列表等核心功能。
/// </summary>
public class BattleController : MonoBehaviour,
    IEventReceiver<GameModeChangedEvent> {
    // 战斗场地管理器，负责根据战斗预加载数据生成战斗单位，并根据当前敌人编队的阵型布局单位位置。
    [SerializeField] private BattleFieldManager fieldManager;
    public BattleFieldManager FieldManager => fieldManager;

    // 战斗实体列表，包含战斗中所有的单位（友方和敌方）。通常在战斗开始时根据预加载数据创建，并在战斗过程中维护更新。
    private readonly List<BattleEntity> _allEntities = new();
    public List<BattleEntity> AllEntities => _allEntities;

    // 当前正在行动的实体，通常由战斗状态机中的“选择行动者”状态设置和更新
    public BattleEntity CurrentEntity { get; set; }
    // 当前正在执行的指令请求，通常由玩家输入状态或AI决策状态设置和更新
    public BattleCommandRequest CurrentCommandRequest { get; set; }

    // 当前战斗状态
    private BattleState _currentState;

    // 战斗是否正在进行中，控制战斗循环的执行与停止
    private bool _battleRunning;
    public bool IsBattleRunning => _battleRunning;

    // 当前正在运行的战斗循环协程
    private Coroutine _battleLoopRoutine; 

    private void OnEnable() {
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }

    #region 事件响应
    public void OnEvent(GameModeChangedEvent evt) {
        if (evt.NewGameMode == GameMode.Battle) {
            StartBattleIfReady();
            return;
        }

        if (_battleRunning) {
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
    public void StopBattle() {
        _battleRunning = false;
        _currentState = null;
        if (_battleLoopRoutine != null) {
            StopCoroutine(_battleLoopRoutine);
            _battleLoopRoutine = null;
        }
    }
}
