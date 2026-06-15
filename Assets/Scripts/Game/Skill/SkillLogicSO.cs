
/// <summary>
/// 技能逻辑策略基类
/// </summary>
public abstract class SkillLogicSO : ScriptableObject {

    /// <summary>
    /// 执行特殊逻辑
    /// </summary>
    /// <param name="controller">控制器</param>
    /// <param name="actor">执行者</param>
    /// <param name="command">命令</param>
    /// <param name="targets">目标</param>
    /// <returns></returns>
    public abstract IEnumerator ExecuteLogic(
        BattleController controller, 
        BattleEntity actor, 
        BattleCommandRequest command,
        List<BattleEntity> targets);
}
