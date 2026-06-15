/// <summary>
/// Attack/Skill 共用执行引擎
/// 负责动作结算与表现，不额外依赖上下文
/// </summary>
public class AttackSkillExecutionEngine {
    #region 运行时缓存
    private BattleController _controller;
    private BattleEntity _actor;
    private BattleCommandRequest _command;
    private List<BattleEntity> _targets;
    private SkillDataSO _skill;
    #endregion

    public IEnumerator Execute(BattleController controller, List<BattleEntity> targets) {
        _controller = controller;
        _actor = controller.CurrentEntity;
        _command = controller.CurrentCommandRequest;
        _targets = targets;
        _skill = _command.Skill;

        if (_skill.skillType == SkillType.Heal) {
            yield return ExecuteHealBranch();
        }
        else if (_skill.damageKind == DamageKind.Magical) {
            yield return ExecuteMagicalBranch();
        }
        else {
            yield return ExecutePhysicalBranch();
        }
    }

    private IEnumerator ExecuteMagicalBranch() {
        yield break;
    }

    private IEnumerator ExecutePhysicalBranch() {
        for(int i = 0; i < _targets.Count; i++) {
            BattleEntity target = _targets[i];

            int damage = target.CalculateDamageFrom(_actor, _skill, 1f);

            for(int hitIndex = 0; hitIndex < _skill.hitCount; hitIndex++) {
                if (!target.IsAlive) break;
                yield return PlayAttackWithWindup();
                ApplyDamageHit(target,damage);
            }
        }
        yield break;
    }

    private IEnumerator ExecuteHealBranch() {
        yield break;
    }

    public void ResetExecutionState() {

    }

    /// <summary>
    /// 根据攻击前摇动画等待时间
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayAttackWithWindup() {
        _actor.Unit.PlayAttackAnimation();
        float windup = _controller.Config.AttackWindupTime;
        if (windup > 0f) {
            yield return new WaitForSeconds(windup);
        }
    }

    /// <summary>
    /// 产生伤害
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="damage">伤害数值</param>
    private void ApplyDamageHit(BattleEntity target, int damage) {
        target.TakeDamage(damage);
        Debug.Log($"{target.Definition.Name} 受到了 {damage} 点伤害");
    }
}
