


using System;

[CreateAssetMenu(menuName = "Character/Enemy", order = 1)]
public class EnemyDefinitionSO : CharacterDefinitionSO
{
    [Header("Rewards")]
    public int ExpReward;
    public int MoneyReward;
    public List<InventoryItem> Drops;

    [Header("弱点及护盾")]
    [Min(1)] public int MaxShields;
    public List<DamageType> Weaknesses;

    [Header("Enemy AI Tuning")]
    [Tooltip("AI行为配置")]public EnemyAITuningConfig aiTuning = EnemyAITuningConfig.CreateDefault();
    [Header("Boss phase")]
    [Tooltip("boss阶段配置")] public List<BossPhaseConfig> BossPhase = new();
}


/// <summary>
/// 敌人AI参调表负责处理AI行为权重、阶段倍率调整等
/// </summary>
[Serializable]
public class EnemyAITuningConfig {
    #region 基础权重
    [Header("Base Weight")]
    [Tooltip("普攻权重"), Min(0)] public float basicAttackWeight;
    [Tooltip("伤害技能权重"), Min(0)] public float damageSkillWeight;
    [Tooltip("特殊技能权重"), Min(0)] public float defaultSkillWeight;
    #endregion

    #region 蓄力必杀阈值
    [Header("Telegraph Threshold")]
    [Tooltip("蓄力必杀所需最低基础伤害"), Min(0f)] public int telgraphMinBasePower;
    [Tooltip("蓄力必杀所需最低Sp消耗"), Min(0f)] public int telgraphMinSpCost;
    [Tooltip("hp比例低于该值时提高蓄力必杀权重"), Range(0f,1f)] public float telgraphHpRatioThreshold;
    [Tooltip("蓄力必杀基础权重"), Min(0f)] public float telgraphWeight;
    #endregion

    #region 治疗策略
    [Header("Heal Strategy")]
    [Tooltip("队友Hp低于该比例判定为残血"),Range(0f,1f)]public float healLowHpRatioThreshold;
    [Tooltip("治疗技能基础权重"),Min(0f)]public float healBaseWeight;
    [Tooltip("每多一个残血队友增加权重"),Min(0f)]public float healPreLowHpBonus;
    [Tooltip("单体治疗在多人残血时的惩罚系数"),Range(0f,1f)]public float singleHealMultiLowHpPenalty;
    #endregion

    public static EnemyAITuningConfig CreateDefault() {
        return new EnemyAITuningConfig {
            basicAttackWeight = 10f,
            damageSkillWeight = 15f,
            defaultSkillWeight = 10f,
            telgraphMinBasePower = 50,
            telgraphMinSpCost = 10,
            telgraphHpRatioThreshold = 0.7f,
            telgraphWeight = 40f,
            healLowHpRatioThreshold = 0.4f,
            healBaseWeight = 40f,
            healPreLowHpBonus = 20f,
            singleHealMultiLowHpPenalty = 0.8f
        };
    }
}

/// <summary>
/// Boss阶段配置
/// </summary>
[Serializable]
public class BossPhaseConfig {
    [Header("Trigger")]
    [Tooltip("血量比例低于该值时触发"),Range(0f,1f)] public float triggerHpRatio = 0.5f;

    [Header("Shield")]
    [Tooltip("是否覆盖原上限")] public bool overrideMaxShield;
    [Tooltip("新的护盾上限"),Min(1)] public int maxShield = 1;

    [Header("Weakness")]
    [Tooltip("是否替换弱点集合")] public bool overrideWeaknesses;
    [Tooltip("新的弱点集合")] public List<DamageType> Weaknesses = new();

    [Header("Prompt")]
    [Tooltip("切换阶段时显示的提示文本"), TextArea] public string promptText;
    [Tooltip("切换阶段时后行动延迟"), Min(0)] public float introDelay = 0.6f;

    [Header("AI Phase Bias")]
    [Tooltip("普攻权重倍率"), Min(0f)] public float basicAttackWeightMultiplier = 1f;
    [Tooltip("伤害技能权重倍率"), Min(0f)] public float damageSkillWeightMultiplier = 1f;
    [Tooltip("治疗技能权重倍率"), Min(0f)] public float healWeightMultiplier = 1f;
    [Tooltip("蓄力必杀权重倍率"), Min(0f)] public float telgraphWeightMultiplier = 1f;

}