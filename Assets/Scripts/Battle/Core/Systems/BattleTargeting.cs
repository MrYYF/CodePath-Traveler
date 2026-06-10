
/// <summary>
/// 管理战斗目标相关规则工具
/// 
/// 某种类型是否进入目标选择状态
/// 某种类型目标应该筛选出哪些候选目标
/// 玩家或AI选择的实体要如何转换回命令里的目标请求参数
/// 避免了不同输入阵营需要各写一套规则的问题
/// 
/// 战斗里所有目标规则的统一工具类
/// </summary>
public static class BattleTargeting {

    public static List<BattleEntity> GetAliveTargetsByType(BattleEntity self, TargetType targetType, List<BattleEntity> allEntities) {
        return targetType switch {
            TargetType.SingleAlly or
            TargetType.AllAllies => allEntities.FindAll(entity => entity.IsAlive && entity.IsPlayer == self.IsPlayer),
            _ => allEntities.FindAll(entity => entity.IsAlive && entity.IsPlayer != self.IsPlayer)
        };
    }
}
