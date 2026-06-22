
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
    private float _vfxHitDelay; // 特效命中延迟
    private DamageType _hitDamageType = DamageType.Untyped;
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
        _hitDamageType = _skill.ResolveDamageType();
        _vfxHitDelay = Mathf.Max(0, _skill.vfxHitDelay);
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

        SkillVfxSpawnMode mode = GetVfxMode();
        bool hasPlayedVfx = false;

        for (int i = 0; i < _targets.Count; i++) {
            var target = _targets[i];
            if (!target.IsAlive) {
                continue;
            }

            // 播放技能特效
            if (mode == SkillVfxSpawnMode.GroupCenter && !hasPlayedVfx) {
                yield return PlayHitVfx(target);
                hasPlayedVfx = true;
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
        SkillVfxSpawnMode mode = GetVfxMode();

        for (int i = 0; i < _targets.Count; i++) {
            bool hasPlayedVfx = false;

            BattleEntity target = _targets[i];
            int damage = target.CalculateDamageFrom(_actor, _skill, _powerMultiplier);

            for (int hitIndex = 0; hitIndex < _hitCount; hitIndex++) {
                if (!target.IsAlive) break;
                yield return PlayAttackWithWindup();

                // 播放技能特效
                if (mode == SkillVfxSpawnMode.GroupCenter && !hasPlayedVfx) {
                    yield return PlayHitVfx(target);
                    hasPlayedVfx = true;
                }

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
        SkillVfxSpawnMode mode = GetVfxMode();
        bool hasPlayedVfx = false;

        for (int i = 0; i < _targets.Count; i++) {
            var target = _targets[i];
            if (!target.IsAlive) {
                continue;
            }

            // 播放技能特效
            if (mode == SkillVfxSpawnMode.GroupCenter && !hasPlayedVfx) {
                yield return PlayHitVfx(target);
                hasPlayedVfx = true;
            }

            target.Heal(healAmount);
            Debug.Log($"[Battle] Heal {target.Definition.Name} 受到了 {healAmount} 点治疗");
            _controller.SpawnDamagePopup(target, healAmount, DamagePopupType.Heal);

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
        // 屏幕震动
        if (_skill.cameraImpluseStrength > 0f) {
            target.Unit.PlayImpulse(_skill.cameraImpluseStrength);
        }

        // 实际造成伤害
        target.TakeDamage(damage);
        Debug.Log($"{target.Definition.Name} 受到了 {damage} 点伤害");

        // 伤害数值弹出
        _controller.SpawnDamagePopup(target, damage, DamagePopupType.Nomal);

        // 计算破盾相关效果
        TryResolveBreakFromHit(target);
    }

    #region 破盾
    /// <summary>
    /// 根据攻击判断是否破盾
    /// </summary>
    /// <param name="target"></param>
    private void TryResolveBreakFromHit(BattleEntity target) {
        if (target.IsPlayer || !target.IsWeakTo(_hitDamageType) || !target.TryReduceShield(1)) {
            return;
        }

        _controller.NotifyEntityBrokenOrDead(target);
    }
    #endregion

    #region VFX
    /// <summary>
    /// 播放技能特效
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerator PlayHitVfx(BattleEntity target) {
        SpawnSkillVfx(target);
        if (_vfxHitDelay > 0) {
            yield return new WaitForSeconds(_vfxHitDelay);
        }
    }

    /// <summary>
    /// 生成技能特效
    /// </summary>
    /// <param name="target"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void SpawnSkillVfx(BattleEntity target) {
        // 获取生成模式
        SkillVfxSpawnMode mode = GetVfxMode();
        bool spawnFromCaster = _skill.vfxSpawnFromCaster;
        Quaternion spawnRot = GetVfxRotation(spawnFromCaster);

        // 生成位置
        Vector3 spawnPos = spawnFromCaster ?
            _actor.Unit.transform.position :
            GetTargetVfxPosition(target, mode);

        // 生成偏移
        Vector3 spawnOffset = spawnFromCaster ?
            spawnRot * _skill.vfxOffset :
            _skill.vfxOffset;

        GameObject vfx = Object.Instantiate(_skill.hitVfxPrefab, spawnPos + spawnOffset, spawnRot);
        float vfxLifetime = Mathf.Max(0, _skill.vfxLifeTime);
        if (vfxLifetime > 0) {
            Object.Destroy(vfx, vfxLifetime);
        }

    }

    /// <summary>
    /// 获取特效生成位置模式
    /// </summary>
    /// <returns></returns>
    private SkillVfxSpawnMode GetVfxMode() {
        if (_skill.vfxSpawnMode == SkillVfxSpawnMode.AutoByTargetType) {
            return _targets.Count > 1 ?
                SkillVfxSpawnMode.Target :
                SkillVfxSpawnMode.GroupCenter;
        }
        return _skill.vfxSpawnMode;
    }

    /// <summary>
    /// 获取特效旋转
    /// </summary>
    /// <param name="spawnFromCaster">是否从施法者位置生成</param>
    /// <returns></returns>
    private Quaternion GetVfxRotation(bool spawnFromCaster) {
        Quaternion baseRot = Quaternion.identity;
        if (spawnFromCaster) {
            baseRot = _actor.Unit.transform.rotation;
        }

        return baseRot * Quaternion.Euler(0, _skill.vfxYRotation, 0);
    }

    /// <summary>
    /// 获取打击点目标位置
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="mode">特效生成模式</param>
    /// <returns>目标打击点位置</returns>
    private Vector3 GetTargetVfxPosition(BattleEntity target, SkillVfxSpawnMode mode) {
        if (mode == SkillVfxSpawnMode.GroupCenter) {
            return _controller.FieldManager.GetSideCenter(ResolveVfxSide());
        }
        return target.Unit.GetPopupAnchorPosition();
    }

    /// <summary>
    /// 获得技能是否是友方释放
    /// </summary>
    /// <returns>true友方释放，false敌方释放</returns>
    private bool ResolveVfxSide() => _skill.targetType switch {
        TargetType.AllAllies or
        TargetType.SingleEnemy or
        TargetType.Self => _actor.IsPlayer,
        _ => !_actor.IsPlayer,
    };

    #endregion
}
