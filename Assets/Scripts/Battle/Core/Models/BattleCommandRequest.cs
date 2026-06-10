
/// <summary>
/// 战斗指令的数据载体,包括指令类型、作用目标、技能/物品/BP点等数据
/// </summary>
public class BattleCommandRequest {
    // 作用对象ID
    public string TargetEntityID;
    // 发出指令的类型
    public BattleCommandType Type;
    public ItemDefinitionSO ItemDefinition;
    public SkillDataSO Skill;
    public int BPSpend;

    public static BattleCommandRequest CreateAttack(BattleEntity actor, int bpSpend = 0) {
        return new BattleCommandRequest {
            Type = BattleCommandType.Attack,
            Skill = actor.Definition.BasicAttack,
            BPSpend = bpSpend
        };
    }

    public static BattleCommandRequest CreateSkill(SkillDataSO skill, int bpSpend = 0) {
        return new BattleCommandRequest {
            Type = BattleCommandType.Skill,
            Skill = skill,
            BPSpend = bpSpend
        };

    }

    public static BattleCommandRequest CreateItem(ItemDefinitionSO item) {
        return new BattleCommandRequest { 
            Type = BattleCommandType.Item,
            ItemDefinition = item
        };
    }

    public static BattleCommandRequest CreateDefend() {
        return new BattleCommandRequest {
            Type = BattleCommandType.Defend,
            TargetEntityID = null
        };
    }

    public static BattleCommandRequest CreateEscape() {
        return new BattleCommandRequest { 
            Type = BattleCommandType.Escape,
            TargetEntityID = null
        };
    }
}
