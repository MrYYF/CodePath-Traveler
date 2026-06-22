/// <summary>
/// 战斗实体类，代表战斗中的一个单位（可以是玩家角色或敌人）。
/// 它封装了角色的运行时数据、定义数据、所属战斗单位、唯一标识符以及是否为玩家等信息。
/// </summary>
public class BattleEntity {
    #region 基础数据
    public CharacterRuntimeData RuntimeData { get; }
    public CharacterDefinitionSO Definition => RuntimeData.Definition;
    public BattleUnit Unit { get; }
    public string ID { get; }
    public bool IsPlayer { get; }
    public bool IsAlive => RuntimeData.CurrentHP > 0;
    public int CurrentHP => RuntimeData.CurrentHP;
    public int CurrentSP => RuntimeData.CurrentSP;
    public int CurrentBP => RuntimeData.CurrentBP;
    public StatBlock TotalStats => RuntimeData.GetTotalStats();
    private const int MaxBattleBP = 5;
    private bool _usedBPInThisTurn = false;
    public bool IsDefending { get; private set; }
    #endregion

    #region 偷取掉落
    public List<InventoryItem> BattleDrops { get; } = new();
    public bool HasBeenRobbed { get; private set; }
    #endregion

    #region 护盾与弱点
    public int CurrentShield { get; private set; }
    public bool IsBroken { get; private set; }
    private int MaxShield { get; set; }
    private int BrokenTurnsRemaining { get; set; } // 剩余需要跳过的行动次数
    private bool BreakSkipPending { get; set; } // 回合内一次性跳过标记，新回合重置为true，消耗BrokenTurnsRemaining重置为false
    private readonly HashSet<DamageType> _weaknesses = new();
    private readonly List<DamageType> _orderedWeaknesses = new();
    #endregion

    #region 阶段状态临时字段
    // 当前阶段的索引
    private int CurrentBossPhaseIndex { get; set; }
    // 已满足阈值但尚未进入的阶段索引
    private int _pendingBossPhaseIndex;
    #endregion

    #region 蓄力技能临时状态字段
    public SkillDataSO PreparedSkill { get; set; }
    #endregion

    public BattleEntity(CharacterRuntimeData runtimeData, BattleUnit unit, bool isPlayer, string stableID) {
        RuntimeData = runtimeData;
        Unit = unit;
        IsPlayer = isPlayer;
        ID = stableID;
        InitializeBattleStats();
    }

    /// <summary>
    /// 初始化实体战斗数据
    /// </summary>
    private void InitializeBattleStats() {
        if (Definition is EnemyDefinitionSO enemyDefinition) {
            InitializeBattleDrops(enemyDefinition);
            MaxShield = Mathf.Max(1, enemyDefinition.MaxShields);
            ApplyWeaknesses(enemyDefinition.Weaknesses, false);
            ResetShieldAndBreakState();
        }
        _usedBPInThisTurn = false;
        CurrentBossPhaseIndex = -1;
        _pendingBossPhaseIndex = -1;
    }


    #region 数值相关方法
    internal int GetCurrentSpeed() {
        return TotalStats.Speed;
    }

    public void SpendBP(int amount) {
        RuntimeData.ModifyBP(-amount);
        //广播更新BP
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentBP, CurrentBP, 5));
    }

    public void RecoverBP() {
        bool shouldRecoverBP = !_usedBPInThisTurn;

        _usedBPInThisTurn = false;

        if (!shouldRecoverBP || CurrentBP >= MaxBattleBP) {
            return;
        }
        RuntimeData.ModifyBP(1);
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentBP, CurrentBP, 5));
    }

    public void SpendSP(int amount) {
        RuntimeData.ModifySP(-amount);
        //广播更新SP
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentSP, CurrentSP, TotalStats.MaxSP));
    }

    public void MarkBPUsed() => _usedBPInThisTurn = true;
    #endregion

    #region 伤害/治疗数值计算
    /// <summary>
    /// 根据攻击者的属性与技能数据计算伤害量
    /// </summary>
    /// <param name="attacker">攻击者实体</param>
    /// <param name="skill">技能数据</param>
    /// <param name="powerMultiplier">威力系数</param>
    /// <returns>伤害量数值</returns>
    public int CalculateDamageFrom(BattleEntity attacker, SkillDataSO skill, float powerMultiplier) {
        bool isMagical = skill != null && skill.damageKind == DamageKind.Magical;
        StatBlock atkStats = attacker.TotalStats;
        StatBlock defStats = TotalStats;

        int atk = isMagical ? atkStats.MAtk : atkStats.PAtk;
        int def = isMagical ? defStats.MDef : defStats.PDef;

        if (IsDefending) {
            def = Mathf.RoundToInt(def * 1.5f);
        }

        int basePower = skill.basePower;
        int rawDamage = Mathf.Max(1, atk - def + basePower);
        return Mathf.RoundToInt(rawDamage * powerMultiplier);
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="amount">伤害数值</param>
    public void TakeDamage(int amount) {
        if (!IsAlive) {
            return;
        }
        // 实际产生伤害
        RuntimeData.ModifyHP(-amount);

        // 检查是否需要阶段转换
        TryQueueBossPhaseTransitionByHp();

        //广播HP变化事件
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentHP, CurrentHP, TotalStats.MaxHP));

        Unit.UpdateVisuals();
    }

    /// <summary>
    /// 根据技能数据计算治疗量
    /// </summary>
    /// <param name="skill">技能数据</param>
    /// <param name="powerMultiplier">威力系数</param>
    /// <returns></returns>
    public int CalculateHealAmountFromSkill(SkillDataSO skill, float powerMultiplier) {
        int baseHeal = Mathf.Max(0, skill.healAmount);
        return Mathf.RoundToInt(baseHeal * powerMultiplier);
    }

    /// <summary>
    /// 应用治疗
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount) {
        if (!IsAlive) {
            return;
        }
        RuntimeData.ModifyHP(amount);
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentHP, CurrentHP, TotalStats.MaxHP));
        Unit.UpdateVisuals();
    }

    /// <summary>
    /// 恢复SP值
    /// </summary>
    /// <param name="amount">恢复数量</param>
    public void restoreSP(int amount) {
        RuntimeData.ModifySP(amount);
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentHP, CurrentHP, TotalStats.MaxHP));
        Unit.UpdateVisuals();

    }

    #endregion

    #region 防御姿态接口
    /// <summary>
    /// 进入防御姿态
    /// </summary>
    public void EnterDefendStance() {
        if (!IsAlive) {
            return;
        }

        IsDefending = true;
    }

    /// <summary>
    /// 退出防御姿态
    /// </summary>
    public void ClearDefendStance() => IsDefending = false;
    #endregion

    #region 掉落/偷取相关
    /// <summary>
    /// 初始化敌方实体战斗掉落
    /// </summary>
    /// <param name="enemyDefinition">敌方数据</param>
    private void InitializeBattleDrops(EnemyDefinitionSO enemyDefinition) {
        BattleDrops.Clear();

        if (enemyDefinition.Drops.Count >= 0) {
            foreach (var drop in enemyDefinition.Drops) {
                if (drop.Quantity <= 0) {
                    continue;
                }
                BattleDrops.Add(new InventoryItem(drop.ItemDefinition, drop.Quantity));
            }
            RefreshRobbedState();
        }
    }

    /// <summary>
    /// 刷新偷取状态
    /// </summary>
    public void RefreshRobbedState() {
        HasBeenRobbed = true;

        foreach (var drop in BattleDrops) {
            if (drop.Quantity > 0) {
                HasBeenRobbed = false;
                return;
            }
        }
    }
    #endregion

    #region 弱点相关
    /// <summary>
    /// 更新弱点列表集合
    /// </summary>
    /// <param name="weaknesses">新的弱点集合</param>
    /// <param name="publishEvent">是否发布弱点更新事件</param>
    private void ApplyWeaknesses(List<DamageType> weaknesses, bool publishEvent) {
        _weaknesses.Clear();
        _orderedWeaknesses.Clear();

        foreach (var type in weaknesses) {
            if (type == DamageType.None || type == DamageType.Untyped) {
                continue;
            }

            // 双写结构：hashset用于命中判定，list用于UI展示
            if (_weaknesses.Add(type)) {
                _orderedWeaknesses.Add(type);
            }
        }

        // 广播弱点变化事件
        if (publishEvent) {
            EventBus.Publish(new EntityWeaknessChangedEvent(this));
        }
    }

    /// <summary>
    /// 伤害类型是否包含弱点
    /// </summary>
    /// <param name="type">伤害类型</param>
    /// <returns>如果非玩家且包含弱点则true，否则false</returns>
    public bool IsWeakTo(DamageType type) => !IsPlayer && _weaknesses.Contains(type);

    /// <summary>
    /// 获取弱点集合
    /// </summary>
    /// <returns></returns>
    public List<DamageType> GetWeaknesses() => _orderedWeaknesses;
    #endregion

    #region 护盾
    /// <summary>
    /// 尝试减少护盾值
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool TryReduceShield(int amount) {
        // 没有破防且有护盾
        if (IsBroken || CurrentShield <= 0) {
            return false;
        }

        // 扣除当前护盾
        CurrentShield = Mathf.Max(0, CurrentShield - amount);

        // 广播
        EventBus.Publish(new EntityShieldChangedEvent(this, CurrentShield));

        // 如果护盾归零则进入破盾状态
        if (CurrentShield == 0) {
            OnBreak();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 进入破盾状态
    /// </summary>
    private void OnBreak() {
        IsBroken = true;
        BrokenTurnsRemaining = 1;
        BreakSkipPending = false;

        // 破盾打断蓄力
        PreparedSkill = null;
        Unit.StopTelegraphVfx();

        // 刷新单位头顶的break眩晕表现
        Unit.SetBreakStunVisual(true);

        // 广播事件
        EventBus.Publish(new EntityBreakEvent(this));
    }

    /// <summary>
    /// 从破盾状态恢复
    /// </summary>
    public void RecoverFromBreak() {
        if (!IsBroken) {
            return;
        }

        // 重置护盾与破盾状态
        ResetShieldAndBreakState();

        // 关闭眩晕表现
        Unit.SetBreakStunVisual(false);

        // 广播事件
        EventBus.Publish(new EntityRecoverFromBreakEvent(this));
        EventBus.Publish(new EntityShieldChangedEvent(this, CurrentShield));
    }

    /// <summary>
    /// 回合开始时从破盾状态恢复
    /// </summary>
    public void ResolveBreakRecoveryAtRoundStart() {
        if (IsBroken && BrokenTurnsRemaining <= 0) {
            RecoverFromBreak();
        }
    }

    /// <summary>
    /// 重置护盾与破盾状态
    /// </summary>
    private void ResetShieldAndBreakState() {
        IsBroken = false;
        BrokenTurnsRemaining = 0;
        BreakSkipPending = false;
        CurrentShield = MaxShield;
    }

    /// <summary>
    /// 增加破盾状态跳过的回合数
    /// </summary>
    /// <param name="amount">回合数</param>
    public void AddBrokenSkipTurns(int amount) {
        if (!IsBroken) {
            return;
        }

        BrokenTurnsRemaining += amount;
    }

    /// <summary>
    /// 消耗剩余需要跳过的行动次数
    /// </summary>
    /// <param name="amount"></param>
    public void ConsumBrokenTurnsByTimeLine(int amount) {
        if (!IsBroken || BrokenTurnsRemaining <= 0) {
            return;
        }

        // 减少剩余破碎回合数
        BrokenTurnsRemaining = Mathf.Max(0, BrokenTurnsRemaining - amount);

        // 下回合若仍需跳过则由BreakSkipPending重置
        BreakSkipPending = false;
    }

    public void TriggerBreakSkipForRound() {
        if (IsBroken && BrokenTurnsRemaining > 0) {
            BreakSkipPending = true;
        }
    }
    #endregion

    #region 阶段逻辑
    /// <summary>
    /// 获取当前阶段配置
    /// </summary>
    /// <returns></returns>
    public BossPhaseConfig GetActiveBossPhaseConfig() {
        if (Definition is not EnemyDefinitionSO enemyDef ||
                CurrentBossPhaseIndex < 0 ||
                CurrentBossPhaseIndex > enemyDef.BossPhase.Count) {
            return null;
        }

        return enemyDef.BossPhase[CurrentBossPhaseIndex];
    }

    /// <summary>
    /// 根据血量比例得到下一个带转换阶段index
    /// </summary>
    private void TryQueueBossPhaseTransitionByHp() {
        if (IsPlayer || !IsAlive || Definition is not EnemyDefinitionSO enemyDef) {
            return;
        }

        float hpRatio = CurrentHP / (float)Mathf.Max(1, TotalStats.MaxHP);
        int targetPhaseIndex = CurrentBossPhaseIndex;

        for (int i = CurrentBossPhaseIndex + 1; i < enemyDef.BossPhase.Count; i++) {
            BossPhaseConfig phase = enemyDef.BossPhase[i];

            if (hpRatio <= phase.triggerHpRatio) {
                targetPhaseIndex = i;
            }
            else {
                break;
            }
        }

        if (targetPhaseIndex > CurrentBossPhaseIndex && targetPhaseIndex > _pendingBossPhaseIndex) {
            _pendingBossPhaseIndex = targetPhaseIndex;
        }
    }

    /// <summary>
    /// 尝试转换到下一阶段
    /// </summary>
    /// <param name="appliedPhase">阶段信息</param>
    /// <returns></returns>
    public bool TryApplyPendingBossPhase(out BossPhaseConfig appliedPhase) {
        appliedPhase = null;

        // 检查 IsBroken 状态
        if (IsBroken) {
            return false;
        }

        // 非敌人 || 无挂起阶段index
        if (Definition is not EnemyDefinitionSO enemyDef || _pendingBossPhaseIndex < 0) {
            return false;
        }

        // 将挂起索引正式赋值，并清空挂起
        BossPhaseConfig phase = enemyDef.BossPhase[_pendingBossPhaseIndex];
        CurrentBossPhaseIndex = _pendingBossPhaseIndex;
        _pendingBossPhaseIndex = -1;
        appliedPhase = phase;

        // 应用配置
        ApplyBossPhaseConfig(phase);
        return true;
    }

    /// <summary>
    /// 套用单个boss阶段配置
    /// </summary>
    /// <param name="phase">阶段配置</param>
    private void ApplyBossPhaseConfig(BossPhaseConfig phase) {
        // 护盾幻化
        if (phase.overrideMaxShield) {
            bool wasBroken = IsBroken;

            MaxShield = phase.maxShield;
            ResetShieldAndBreakState();
            Unit.SetBreakStunVisual(false);

            if (wasBroken) {
                EventBus.Publish(new EntityRecoverFromBreakEvent(this));
            }
            EventBus.Publish(new EntityShieldChangedEvent(this, CurrentShield));
        }

        // 弱点变化
        if (phase.overrideWeaknesses) {
            ApplyWeaknesses(phase.Weaknesses, true);
        }
    }

    /// <summary>
    /// 阶段转换提示信息
    /// </summary>
    /// <param name="phase">阶段配置</param>
    /// <returns>提示信息文字</returns>
    public string ResolveBossPhasePrompt(BossPhaseConfig phase) =>
        !string.IsNullOrWhiteSpace(phase.promptText) ?
        phase.promptText :
        "进入了新阶段！";
    #endregion
}
