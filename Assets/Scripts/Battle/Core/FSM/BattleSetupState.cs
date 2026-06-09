/// <summary>
/// 战斗开始状态
/// <remarks>
/// 该状态主要负责：
/// 1. 读取BattleStartPreload中的数据，创建战斗实体并将其添加到BattleController的实体列表中。
/// 2. 绑定 RuntimeData、BattleEntity 和 BattleUnit 之间的关系，确保它们能够正确交互。
/// 3. 播放最小开场入场表现
/// 4. 初始化首轮时间轴，切换到“下一位行动者”状态
/// 
/// 可以理解为：
/// “战斗开始后的总装配状态”
/// </remarks>
/// </summary>
public class BattleSetupState : BattleState {

    private readonly BattleStartPreload _startPreload;
    public BattleSetupState(BattleController contorller, BattleStartPreload startPreload) : base(contorller) {
        _startPreload = startPreload;
    }

    public override IEnumerator Execute() {
        // 生成敌我双方的BattleUnit
        _controller.FieldManager.SpawnAll(_startPreload);

        // 绑定RuntimeData、BattleEntity和BattleUnit之间的关系
        _controller.AllEntities.Clear();
        List<BattleEntity> allyEntities = CreateEntities(_startPreload.allies, _controller.FieldManager.SpawnedAllyUnits, true);
        _controller.AllEntities.AddRange(allyEntities);
        _controller.AllEntities.AddRange(CreateEntities(_startPreload.enemy, _controller.FieldManager.SpawnedEnemyUnits, false));

        // 敌方出现在默认位置，友方从屏幕外飞入
        float runtime = 2f; // TODO:测试效果用，后续需要做成统一SO配置文件
        if (runtime > 0f) {
            yield return new WaitForSeconds(runtime);

            foreach (var entity in allyEntities) {
                Vector3 homePos = _controller.FieldManager.GetHomePos(entity.Unit);
                _controller.StartCoroutine(entity.Unit.MoveToPosition(homePos, runtime));
            }

            yield return new WaitForSeconds(runtime);
        }

        // 通知HUD建立显示，初始化首轮时间轴
        EventBus.Publish(new BattleStartedEvent());
        _controller.StartNewRound();
        _controller.UpdateTimelinePrediction();

        yield return new WaitForSeconds(runtime);

        _controller.SetState(new SelectNextEntityState(_controller));
        yield return null;
    }

    /// <summary>
    /// 根据RuntimeData和BattleUnit创建BattleEntity，并将它们绑定在一起
    /// </summary>
    /// <param name="runtimeList">实时数据</param>
    /// <param name="units">游戏单位</param>
    /// <param name="isAlly">是否为友方</param>
    /// <returns></returns>
    private List<BattleEntity> CreateEntities(List<CharacterRuntimeData> runtimeList, IReadOnlyList<BattleUnit> units, bool isAlly) {
        List<BattleEntity> entities = new List<BattleEntity>(runtimeList.Count);
        string sidePrefix = isAlly ? "Ally" : "Enemy";

        for (int i = 0; i < runtimeList.Count; i++) {
            CharacterRuntimeData runtimeData = runtimeList[i];
            BattleUnit unit = units[i];
            BattleEntity entity = new BattleEntity(runtimeData, unit, isAlly, $"{sidePrefix}_{i:D2}_{runtimeData.Definition.ID}");
            unit.Bind(entity);
            entities.Add(entity);

        }

        return entities;
    }

}
