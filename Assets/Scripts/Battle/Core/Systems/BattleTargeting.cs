
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
public static class BattleTargeting
{
    /// <summary>
    /// 根据目标类型收集候选实体集合
    /// </summary>
    /// <param name="actor">当前行动对象</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="allEntities">全部实体集合</param>
    /// <returns>候选实体集合</returns>
    public static List<BattleEntity> CollectTargets(BattleEntity actor, TargetType targetType, List<BattleEntity> allEntities) {
        List<BattleEntity> result = new List<BattleEntity>();
        if(targetType == TargetType.Self) {
            result.Add(actor);
            return result;
        }
        // 目标是否是友方
        bool targetAllies = targetType == TargetType.SingleAlly || targetType == TargetType.AllAllies;
        // 目标是否是玩家阵营
        bool targetPlayerSide = targetAllies ? actor.IsPlayer : !actor.IsPlayer;

        for (int i = 0; i < allEntities.Count; i++) {
            BattleEntity entity = allEntities[i];
            if(entity.IsAlive && entity.IsPlayer == targetPlayerSide) {
                result.Add(entity);
            }
        }

        return result;
    }

    /// <summary>
    /// 将单体选中结果转化为BattleTargetRequest，供命令层统一处理
    /// </summary>
    /// <param name="actor">当前行动对象</param>
    /// <param name="selectedTarget">选中的目标实体</param>
    /// <returns></returns>
    public static BattleTargetRequest BuildSingleTargetRequest(BattleEntity actor, BattleEntity selectedTarget) {

        if(selectedTarget == actor) {
            return BattleTargetRequest.Self(actor.ID);
        }

        return selectedTarget.IsPlayer == actor.IsPlayer ?
            BattleTargetRequest.SingleAlly(selectedTarget.ID) :
            BattleTargetRequest.SingleEnemy(selectedTarget.ID);
    }
}
