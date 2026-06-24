using System.Linq;

public class PlayerEncounterTracker : MonoBehaviour {

    private EncounterZone _currentZone;
    private Vector3 _lastPosition;

    // 距离计数器
    private float _accumulatedDistance;
    private float _targetEncounterDistance;


    private void Start() {
        _lastPosition = transform.position;
    }

    private void Update() {
        Vector3 currentPos = transform.position;
        if (_currentZone == null) {
            _lastPosition = currentPos;
            return;
        }

        // 计算水平移动距离
        Vector3 horizontalDelta = currentPos - _lastPosition;
        horizontalDelta.y = 0;
        float distanceMoved = horizontalDelta.magnitude;
        _lastPosition = currentPos;

        if (distanceMoved <= 0.01f) {
            return;
        }

        // 获取累计移动距离
        _accumulatedDistance += distanceMoved;
        if (_accumulatedDistance >= _targetEncounterDistance) {
            TriggerEncounter();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.TryGetComponent(out EncounterZone zone))
            return;

        _currentZone = zone;
        _lastPosition = transform.position;
        // 重置遇敌
        ResetEncounterDistance(zone);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.TryGetComponent(out EncounterZone zone))
            return;

        if (_currentZone != zone)
            return;

        _currentZone = null;
        _lastPosition = transform.position;
    }

    /// <summary>
    /// 重置遇敌距离数据
    /// </summary>
    /// <param name="zone">遇敌区域</param>
    private void ResetEncounterDistance(EncounterZone zone) {
        _accumulatedDistance = 0;
        _targetEncounterDistance = Random.Range(zone.minEncounterDistance, zone.maxEncounterDistance);
    }

    /// <summary>
    /// 触发遇敌
    /// </summary>
    private void TriggerEncounter() {
        GameModeManager.Instance.RequestChangeGameMode(GameMode.InteractionMenu);
        // 准备开始战斗
        StartCoroutine(StartBattleRoutine(_currentZone));

    }

    /// <summary>
    /// 准备开始战斗协程
    /// </summary>
    /// <param name="zone"></param>
    /// <returns></returns>
    private IEnumerator StartBattleRoutine(EncounterZone zone) {
        yield return new WaitForSeconds(2f);

        // 抽取遇敌配置信息
        EncounterGroup encounter = zone.GetRandomEncounter();

        // 获取战斗开始前预加载数据
        List<CharacterRuntimeData> allies = new(PartyManager.Instance.PartyMembers);
        List<CharacterRuntimeData> enemyDef = encounter.Enemies
            .Select(enemy => new CharacterRuntimeData(enemy))
            .ToList();

        // 触发战斗
        BattleService.Instance.StartBattle(allies, enemyDef, zone.battleSceneReference, encounter.Fomation);

        ResetEncounterDistance(zone);
    }

    /// <summary>
    /// 传送后重置遇敌计数状态
    /// </summary>
    /// <param name="position"></param>
    public void ResetEncounterTracking(Vector3 position) {
        _lastPosition = position;
        _accumulatedDistance = 0f;
    }
}
