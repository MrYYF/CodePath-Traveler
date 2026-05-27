

/// <summary>
/// 战斗开始预加载数据，包含战斗开始前需要准备的角色数据和敌人布阵信息。
/// </summary>
public class BattleStartPreload {
    public List<CharacterRuntimeData> allies { get; }
    public List<CharacterRuntimeData> enemy { get; }

    public EnemyLayoutFomation enemyLayoutFomation { get; }

    public BattleStartPreload(List<CharacterRuntimeData> allies,
        List<CharacterRuntimeData> enemy,
        EnemyLayoutFomation enemyLayoutFomation) {
        this.allies = allies;
        this.enemy = enemy;
        this.enemyLayoutFomation = enemyLayoutFomation;
    }
}
