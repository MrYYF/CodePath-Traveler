
/// <summary>
/// 战斗指令的数据载体,包括指令类型、作用目标请求数据
/// </summary>
public class BattleCommandRequest {
    // 发出指令的类型
    public BattleCommandType Type;
    // 作用对象
    public BattleTargetRequest Target;
    public SkillDataSO Skill;

    public static BattleCommandRequest CreateAttack(BattleTargetRequest target) {
        return new BattleCommandRequest {
            Target = target,
            Type = BattleCommandType.Attack
        };
    }

    public static BattleCommandRequest CreateSkill(BattleTargetRequest target, SkillDataSO skill) {
        return new BattleCommandRequest {
            Target = target,
            Type = BattleCommandType.Skill,
            Skill = skill
        };

    }

    public static BattleCommandRequest CreateItem() {
        return new BattleCommandRequest { Type = BattleCommandType.Item };
    }

    public static BattleCommandRequest CreateDefend() {
        return new BattleCommandRequest { Type = BattleCommandType.Defend };
    }

    public static BattleCommandRequest CreateEscape() {
        return new BattleCommandRequest { Type = BattleCommandType.Escape };
    }
}
