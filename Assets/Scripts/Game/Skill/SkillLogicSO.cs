
/// <summary>
/// 技能逻辑策略基类
/// </summary>
public abstract class SkillLogicSO : ScriptableObject {

    /// <summary>
    /// 执行特殊逻辑
    /// </summary>
    /// <param name="context">战斗上下文（施法者、控制器）</param>
    /// <param name="targets">已选定的目标列表</param>
    /// <param name="skillData">技能数据</param>
    /// <returns></returns>
    public abstract IEnumerator ExecuteLogic(BattleActionContext context, List<BattleEntity> targets, SkillDataSO skillData);
}
