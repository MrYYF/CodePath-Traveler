

using System;

/// <summary>
/// 技能
/// </summary>
[CreateAssetMenu(menuName = "Battle/Skill")]
public class SkillDataSO : ScriptableObject {
    [Header("Identify")]
    public string skillID;
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    [Min(0)] public int spCost;

    [Header("Type")]
    public TargetType targetType = TargetType.SingleEnemy;
    public SkillType skillType = SkillType.Damage;
    public DamageKind damageKind = DamageKind.Physical;
    public ElementType elementType = ElementType.None;
    public WeaponType weaponType = WeaponType.None;

    [Header("Effect")]
    [Min(0)] public int basePower;
    [Min(1)] public int hitCount = 1;
    [Min(0)] public int healAmount;

    [Header("Special Logic Strategy")]
    public SkillLogicSO specialLogic;

    [Header("Boost")]
    public BoostTierConfig[] boostTiers = new BoostTierConfig[4] {
        BoostTierConfig.Default(0),
        BoostTierConfig.Default(1),
        BoostTierConfig.Default(2),
        BoostTierConfig.Default(3)
    };

    [Header("VFX")]
    public GameObject hitVfxPrefab;
    public SkillVfxSpawnMode vfxSpawnMode = SkillVfxSpawnMode.AutoByTargetType;
    [Tooltip("勾选后特效会从施法者当前位置发出，否则沿用SpawnMode设定")]
    public bool vfxSpawnFromCaster = false;
    public Vector3 vfxOffset;
    public float vfxYRotation = 0f;
    [Tooltip("命中延迟"), Min(0)] public float vfxHitDelay = 0f;
    [Tooltip("销毁延迟"), Min(0)] public float vfxLifeTime = 2f;
    [Header("镜头效果")]
    [Min(0f)] public float cameraImpluseStrength = 0f;


    #region BP相关方法
    /// <summary>
    /// 根据bp点数的等级获取技能boost加成配置
    /// </summary>
    /// <param name="boostLevel">bp等级</param>
    /// <returns>boost配置</returns>
    public BoostTierConfig GetBoostTier(int boostLevel) {
        int t = Mathf.Clamp(boostLevel, 0, 3);
        return boostTiers[t];
    }

    /// <summary>
    /// 根据bp点数获取boost修正系数
    /// </summary>
    /// <param name="bpSpend">bp点数</param>
    /// <returns>修正系数</returns>
    public float GetBoostPowerMultiplier(int bpSpend) {
        int spend = Mathf.Clamp(bpSpend, 0, 3);
        return GetBoostTier(spend).powerMultiplier;
    }

    /// <summary>
    /// 获取最终的打击次数
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="bpSpend">bp点数</param>
    /// <returns>打击次数</returns>
    public int GetFinalHitCount(BattleCommandType commandType, int bpSpend) {
        if (commandType == BattleCommandType.Attack && bpSpend <= 0) {
            return 1;
        }
        int finalHitCount = hitCount;
        int spend = Mathf.Clamp(bpSpend, 0, 3);
        return finalHitCount + GetBoostTier(spend).hitCountBonus;
    }
    #endregion

    public DamageType ResolveDamageType() {
        if (weaponType != WeaponType.None) {
            return weaponType switch {
                WeaponType.Sword => DamageType.Sword,
                WeaponType.Bow => DamageType.Bow,
                WeaponType.Dagger => DamageType.Dagger,
                WeaponType.Axe => DamageType.Axe,
                WeaponType.Spear => DamageType.Spear,
                WeaponType.Staff => DamageType.Staff,
                _ => DamageType.Untyped
            };
        }

        if (elementType != ElementType.None) {
            return elementType switch {
                ElementType.Fire => DamageType.Fire,
                ElementType.Ice => DamageType.Ice,
                ElementType.Wind => DamageType.Wind,
                ElementType.Dark => DamageType.Dark,
                ElementType.Light => DamageType.Light,
                ElementType.Lightning => DamageType.Lightning,
                _ => DamageType.Untyped
            };
        }

        return DamageType.Untyped;
    }
}

/// <summary>
/// Boost 分层配置结构
/// </summary>
[Serializable]
public struct BoostTierConfig {
    [Tooltip("Boost 等级（0~3）"), Range(0, 3)] public int tier;

    [Header("Combat Stats")]
    [Tooltip("修正系数"), Min(0.01f)] public float powerMultiplier;
    [Tooltip("命中次数加成"), Min(0)] public int hitCountBonus;

    [Header("Utility Stats")]
    [Tooltip("概率加成")] public float chanceBonus;
    [Tooltip("持续回合加成")] public int durationBonus;
    [Tooltip("通用数值加成")] public int genericValueBonus;

    public static BoostTierConfig Default(int tier) {
        return new BoostTierConfig {
            tier = tier,
            powerMultiplier = 1f,
            hitCountBonus = 0,
            chanceBonus = 0,
            durationBonus = 0,
            genericValueBonus = 0,
        };
    }
}