
using System;

/// <summary>
/// 战斗目标请求数据
/// 记录命令最终作用对象
/// </summary>
public class BattleTargetRequest {
    // 攻击对象
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


}
