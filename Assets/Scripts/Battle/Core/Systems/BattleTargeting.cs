
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

    /// <summary>
    /// 根据当前行动对象以及目标类型返回阵营实体集合
    /// </summary>
    /// <param name="self">当前行动的实体</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="allEntities">场上所有实体</param>
    /// <returns></returns>
    public static List<BattleEntity> GetAliveTargetsByType(BattleEntity self, TargetType targetType, List<BattleEntity> allEntities) {
        return targetType switch {
            TargetType.SingleAlly or
            TargetType.AllAllies => allEntities.FindAll(entity => entity.IsAlive && entity.IsPlayer == self.IsPlayer),
            _ => allEntities.FindAll(entity => entity.IsAlive && entity.IsPlayer != self.IsPlayer)
        };
    }

    /// <summary>
    /// 获取当前命令所对应的目标实体集合
    /// </summary>
    /// <param name="controller">战斗控制器</param>
    /// <returns>被选中的目标集合</returns>
    public static List<BattleEntity> BuildExecutionTargets(BattleController controller) {
        BattleEntity actor = controller.CurrentEntity;
        BattleCommandRequest command = controller.CurrentCommandRequest;
        List<BattleEntity> allEntities = controller.AllEntities;

        if (command.Type == BattleCommandType.Defend || command.Type == BattleCommandType.Escape) {

            return new List<BattleEntity>() { actor };
        }

        if (command.Type == BattleCommandType.Item) {
            BattleEntity itemTarget = allEntities.Find(entity => entity.IsAlive && entity.ID == command.TargetEntityID);
            return itemTarget != null ? new List<BattleEntity> { itemTarget } : new List<BattleEntity>();
        }

        if (command.Type == BattleCommandType.Attack || command.Type == BattleCommandType.Skill) {
            TargetType targetType = command.Skill.targetType;
            if (targetType == TargetType.SingleEnemy || targetType == TargetType.SingleAlly) {
                BattleEntity target = allEntities.Find(entity => entity.IsAlive && entity.ID == command.TargetEntityID);
                return target != null ? new List<BattleEntity>() { target } : new List<BattleEntity>();
            }
            return GetAliveTargetsByType(actor, targetType, allEntities);
        }

        return new List<BattleEntity>();
    }
}
