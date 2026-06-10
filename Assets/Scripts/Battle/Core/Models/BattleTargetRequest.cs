/// <summary>
/// 战斗目标请求数据
/// 记录命令最终作用对象的ID、攻击类型等数据
/// </summary>
public class BattleTargetRequest {
    // 攻击对象ID
    public string TargetEntityID;
    // 攻击类型
    public TargetType TargetType;
    // 是否有目标
    public bool HasTargetEntity => !string.IsNullOrEmpty(TargetEntityID);

    public static BattleTargetRequest FromType(TargetType type) => new BattleTargetRequest { TargetType = type };
    
    public static BattleTargetRequest SingleEnemy(string id) {
        return new BattleTargetRequest {
            TargetType = TargetType.SingleEnemy,
            TargetEntityID = id
        };
    }
    public static BattleTargetRequest SingleAlly(string id) {
        return new BattleTargetRequest {
            TargetType = TargetType.SingleAlly,
            TargetEntityID = id
        };
    }

    public static BattleTargetRequest Self(string id) {
        return new BattleTargetRequest {
            TargetType = TargetType.Self,
            TargetEntityID = id
        };
    }

    public static BattleTargetRequest AllEnemies =>
        new BattleTargetRequest {
            TargetType = TargetType.AllEnemies,
        };

    public static BattleTargetRequest AllAllies =>
        new BattleTargetRequest {
            TargetType = TargetType.AllAllies,
        };
}
