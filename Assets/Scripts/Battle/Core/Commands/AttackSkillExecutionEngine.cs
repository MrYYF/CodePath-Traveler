/// <summary>
/// Attack/Skill 共用执行引擎
/// 负责动作结算与表现，不额外依赖上下文
/// </summary>
public class AttackSkillExecutionEngine
{
    #region 运行时缓存
    private BattleController _controller;
    private BattleEntity _actor;
    private BattleCommandRequest _command;
    private List<BattleEntity> _targets;
    private SkillDataSO _skill;
    #endregion

    public IEnumerator Execute(BattleController controller,List<BattleEntity> targets) {
        _controller = controller;
        _actor = controller.CurrentEntity;
        _command = controller.CurrentCommandRequest;
        _targets = targets;
        _skill = _command.Skill;

        if(_skill.skillType == SkillType.Heal) {
            yield return ExecuteHealBranch();
            yield break;
        }

        yield return ExecuteDamageBranch();
    }

    private IEnumerator ExecuteDamageBranch() {
        yield break;
    }

    private IEnumerator ExecuteHealBranch() {
        yield break;
    }

    public void ResetExecutionState() {

    }
}
