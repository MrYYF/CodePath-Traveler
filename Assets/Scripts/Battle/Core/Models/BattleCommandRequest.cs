
/// <summary>
/// 战斗指令的数据载体
/// </summary>
public class BattleCommandRequest {
    // 发出指令的类型
    public BattleCommandType Type;
    // 作用对象
    public BattleTargetRequest Target;

    public static BattleCommandRequest CreateAttack(BattleTargetRequest target) {
        return new BattleCommandRequest {
            Target = target,
            Type = BattleCommandType.Attack
        };
    }

    public static BattleCommandRequest CreateSkill() {
        return new BattleCommandRequest { Type = BattleCommandType.Skill };
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
