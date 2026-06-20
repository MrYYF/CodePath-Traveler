/// <summary>
/// 战斗场地管理器，负责根据战斗预加载数据生成战斗单位，并根据当前敌人编队的阵型布局单位位置。
/// </summary>
public class BattleFieldManager : MonoBehaviour {

    [SerializeField] private BattleUnit battleUnitPrefab;
    private BattleFieldLayout layout;

    private Transform allyRoot;
    private Transform enemyRoot;

    [Header("Boost VFX")]
    [SerializeField] private GameObject[] boostVfxPrefabs;
    [Tooltip("偏移量"), SerializeField] private Vector3 boostVfxOffset;
    private GameObject _boostVfxInstance;
    private int _currentBoostVfxLevel;

    // 单位与其初始位置的映射关系，用于重置单位位置等操作
    private readonly Dictionary<BattleUnit, Vector3> _homePos = new();

    private readonly List<BattleUnit> _spawnedAllyUnits = new();
    private readonly List<BattleUnit> _spawnedEnemyUnits = new();
    public IReadOnlyList<BattleUnit> SpawnedAllyUnits => _spawnedAllyUnits;
    public IReadOnlyList<BattleUnit> SpawnedEnemyUnits => _spawnedEnemyUnits;

    // 当前敌人编队的阵型
    private EnemyLayoutFomation _currentFomation = EnemyLayoutFomation.Line;

    /// <summary>
    /// 生成所有单位
    /// </summary>
    /// <param name="preload">战斗预加载数据</param>
    public void SpawnAll(BattleStartPreload preload) {
        layout = FindAnyObjectByType<BattleFieldLayout>();

        allyRoot = new GameObject("AllyRoot").transform;
        allyRoot.SetParent(layout.transform, false);

        enemyRoot = new GameObject("EnemyRoot").transform;
        enemyRoot.SetParent(layout.transform, false);

        _currentFomation = preload.enemyLayoutFomation;

        // 清空已生成的单位
        ClearAllUnits();

        // 生成单位
        _spawnedAllyUnits.AddRange(SpawnSide(preload.allies.Count, true));
        _spawnedEnemyUnits.AddRange(SpawnSide(preload.enemy.Count, false));
    }

    /// <summary>
    /// 根据数量和阵营生成单位
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="isAlly">是否为友军</param>
    /// <returns>生成的单位列表</returns>
    private List<BattleUnit> SpawnSide(int count, bool isAlly) {
        List<BattleUnit> units = new List<BattleUnit>();
        for (int i = 0; i < count; i++) {
            // 根据是友军还是敌军，获取对应的单位坐标位置
            Vector3 targetSlotPos = isAlly ?
                layout.GetAllySlotPos(i, count) :
                layout.GetEnemySlotPos(i, count, _currentFomation);

            // 决定出生点
            Vector3 spawnPos = isAlly ?
                layout.initTrans.position :
                targetSlotPos;

            // 生成单位
            BattleUnit unitObj = Instantiate(
                battleUnitPrefab,
                spawnPos,
                Quaternion.identity,
                isAlly ? allyRoot : enemyRoot);

            units.Add(unitObj);

            _homePos[unitObj] = targetSlotPos;

        }


        return units;
    }

    /// <summary>
    /// 清除所有已生成的单位
    /// </summary>
    private void ClearAllUnits() {
        foreach (var unit in _spawnedAllyUnits) {
            Destroy(unit.gameObject);
        }
        _spawnedAllyUnits.Clear();
        foreach (var unit in _spawnedEnemyUnits) {
            Destroy(unit.gameObject);
        }
        _spawnedEnemyUnits.Clear();

        _homePos.Clear();
    }

    public Vector3 GetHomePos(BattleUnit unit) => _homePos[unit];
    public Vector3 GetActionPos(BattleUnit unit) => layout.actionTrans.position;

    #region boost VFX
    public void SetBoostVfxLevel(int level) {
        if (_currentBoostVfxLevel == level) {
            return;
        }

        _currentBoostVfxLevel = level;

        if (_boostVfxInstance != null) {
            Destroy(_boostVfxInstance);
        }

        if (level == 0) {
            return;
        }

        if (level > 0) {
            _boostVfxInstance = Instantiate(
                boostVfxPrefabs[level - 1],
                layout.actionTrans.position,
                Quaternion.identity,
                layout.actionTrans
                );

            _boostVfxInstance.transform.localPosition = boostVfxOffset;
        }
    }
    #endregion
}
