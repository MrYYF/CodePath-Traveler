
/// <summary>
/// 时间轴预测节点
/// 
/// 把调度器排序好的结果整理成UI可以直接消费的一份最小数据
/// 这样UI层就不需要直接依赖调度器的内部列表数据
/// </summary>
public readonly struct BattleTimelinePredictionNode {
    // 用于保证同一角色跨回合预测时图标也能稳定复用
    public readonly string UniqueID;
    // 预测对应的单位实体
    public readonly BattleEntity Entity;
    // 0表示当前回合，1表示下个回合
    public readonly int Round;

    public BattleTimelinePredictionNode(string uniqueID, BattleEntity entity, int round) {
        UniqueID = uniqueID;
        Entity = entity;
        Round = round;
    }
}
