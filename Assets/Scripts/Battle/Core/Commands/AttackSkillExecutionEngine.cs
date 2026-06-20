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

    #region 执行参数缓存
    private float _powerMultiplier = 1f; // 威力系数
    private int _hitCount = 1; // 打击次数
    private float _groupInterval; // 群体间隔
    private float _hitInterval; // 打击间隔
    #endregion

    /// <summary>
    /// 缓存执行参数
    /// </summary>
    private void CacheExecutionParameters() {
        bool isPhysicalBranch = _skill.damageKind == DamageKind.Physical
            && _skill.skillType != SkillType.Heal;

        _powerMultiplier = _skill.GetBoostPowerMultiplier(_command.BPSpend);
        _hitCount = isPhysicalBranch ? _skill.GetFinalHitCount(_command.Type, _command.BPSpend) : 1;
        _groupInterval = _targets.Count > 1 ? _controller.Config.GroupTargetHitInterval : 0f;
        _hitInterval = _hitCount > 1 ? _controller.Config.MultiHitInterval : 0f;
    }

    public IEnumerator Execute(BattleController controller, List<BattleEntity> targets) {
        _controller = controller;
        _actor = controller.CurrentEntity;
        _command = controller.CurrentCommandRequest;
        _targets = targets;
        _skill = _command.Skill;

        // 特殊技能单独逻辑分支
        if (_skill.specialLogic != null) {
            yield return PlayAttackWithWindup();
            yield return _skill.specialLogic.ExecuteLogic(_controller, _actor, _command, _targets); ;
            yield break;
        }

        CacheExecutionParameters();

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

    /// <summary>
    /// 魔法攻击分支
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExecuteMagicalBranch() {
        // 播放前摇动画
        yield return PlayAttackWithWindup();

        for (int i = 0; i < _targets.Count; i++) {
            var target = _targets[i];
            if (!target.IsAlive) {
                continue;
            }
            int damage = target.CalculateDamageFrom(_actor, _skill, _powerMultiplier);
            ApplyDamageHit(target, damage);

            // 群体攻击等待间隔
            if (_groupInterval > 0f && i < _targets.Count - 1) {
                yield return new WaitForSeconds(_groupInterval);
            }
        }
    }

    /// <summary>
    /// 物理攻击分支
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExecutePhysicalBranch() {
        for (int i = 0; i < _targets.Count; i++) {
            BattleEntity target = _targets[i];

            int damage = target.CalculateDamageFrom(_actor, _skill, _powerMultiplier);

            for (int hitIndex = 0; hitIndex < _hitCount; hitIndex++) {
                if (!target.IsAlive) break;
                yield return PlayAttackWithWindup();
                ApplyDamageHit(target, damage);

                // 多段攻击等待间隔
                if (_hitInterval > 0f && hitIndex < _hitCount - 1) {
                    yield return new WaitForSeconds(_hitInterval);
                }
            }

            // 群体目标等待间隔
            if (_groupInterval > 0f && i < _targets.Count - 1) {
                yield return new WaitForSeconds(_groupInterval);
            }
        }
        yield break;
    }

    /// <summary>
    /// 治疗行动分支
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExecuteHealBranch() {
        // 播放前摇动画
        yield return PlayAttackWithWindup();
        int healAmount = _actor.CalculateHealAmountFromSkill(_skill, _powerMultiplier);

        for (int i = 0; i < _targets.Count; i++) {
            var target = _targets[i];
            if (!target.IsAlive) {
                continue;
            }

            target.Heal(healAmount);
            Debug.Log($"[Battle] Heal {target.Definition.Name} 受到了 {healAmount} 点治疗");
            _controller.SpawnDamagePopup(target, healAmount, DamageType.Heal);

            // 群体目标等待间隔
            if (_groupInterval > 0f && i < _targets.Count - 1) {
                yield return new WaitForSeconds(_groupInterval);
            }
        }

    }

    /// <summary>
    /// 重置执行参数
    /// </summary>
    public void ResetExecutionState() {
        _powerMultiplier = 1f;
        _hitCount = 1;
        _groupInterval = 0;
        _hitInterval = 0;
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
        _controller.SpawnDamagePopup(target, damage, DamageType.Nomal);
    }
}
