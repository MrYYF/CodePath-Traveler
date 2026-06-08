using System;

/// <summary>
/// 战斗实体类，代表战斗中的一个单位（可以是玩家角色或敌人）。
/// 它封装了角色的运行时数据、定义数据、所属战斗单位、唯一标识符以及是否为玩家等信息。
/// </summary>
public class BattleEntity {


    public CharacterRuntimeData RuntimeData { get; }
    public CharacterDefinitionSO Definition => RuntimeData.Definition;
    public BattleUnit Unit { get; }
    public string ID { get; }
    public bool IsPlayer { get; }
    public bool IsAlive => RuntimeData.CurrentHP > 0;
    public int CurrentHP => RuntimeData.CurrentHP;
    public StatBlock TotalStats => RuntimeData.GetTotalStats();

    public BattleEntity(CharacterRuntimeData runtimeData, BattleUnit unit, bool isPlayer, string stableID) {
        RuntimeData = runtimeData;
        Unit = unit;
        IsPlayer = isPlayer;
        ID = stableID;
    }
}
