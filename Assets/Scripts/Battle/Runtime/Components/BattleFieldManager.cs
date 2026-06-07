public class BattleFieldManager : MonoBehaviour {

    [SerializeField] private BattleUnit battleUnitPrefab;
    private BattleFieldLayout layout;

    private readonly List<BattleUnit> _spawnedAllyUnits = new();
    private readonly List<BattleUnit> _spawnedEnemyUnits = new();
    private EnemyLayoutFomation _currentFomation = EnemyLayoutFomation.Line;
}
