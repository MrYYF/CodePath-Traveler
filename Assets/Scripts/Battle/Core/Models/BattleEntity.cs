using System;
using static UnityEngine.EventSystems.EventTrigger;

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
    private int MaxShield { get; set; }
    private readonly HashSet<DamageType> _weaknesses = new();
    private readonly List<DamageType> _orderedWeaknesses = new();
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
        }
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
        if (_usedBPInThisTurn || CurrentBP >= MaxBattleBP) {
            _usedBPInThisTurn = false;
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

        RuntimeData.ModifyHP(-amount);

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

    #region 掉落、偷取相关
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

    #region 护盾、弱点相关
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

    public List<DamageType> GetWeaknesses() => _orderedWeaknesses;
    #endregion
}
