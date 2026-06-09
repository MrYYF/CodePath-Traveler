

using System;

public abstract class CharacterDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public string ID;
    public string Name;
    public Sprite Portrait; // 人物立绘
    public Job Job; // 职业

    [Header("Stats")]
    public int BaseLevel = 1; // 基础等级
    public StatBlock BaseStats; // 基础属性

    [Header("Skill")]
    public SkillDataSO BasicAttack;
    public List<SkillDataSO> InitialSkills; // 初始技能 

    [Header("Animator")]
    public AnimatorOverrideController FieldAnimator;
    public AnimatorOverrideController BattleAnimator;

}

[Serializable]
public struct StatBlock {
    public int MaxHP; // 最大生命值
    public int MaxSP; // 最大法力值
    public int PAtk; // 物理攻击
    public int PDef; // 物理防御
    public int MAtk; // 魔法攻击
    public int MDef; // 魔法防御
    public int Speed; // 速度
    public int Accuracy; // 命中
    public int Evasion; // 闪避
    // 可以根据需要添加更多属性

    public static StatBlock Zero = new();

    public static StatBlock operator +(StatBlock a, StatBlock b) {
        return new StatBlock {
            MaxHP = a.MaxHP + b.MaxHP,
            MaxSP = a.MaxSP + b.MaxSP,
            PAtk = a.PAtk + b.PAtk,
            PDef = a.PDef + b.PDef,
            MAtk = a.MAtk + b.MAtk,
            MDef = a.MDef + b.MDef,
            Speed = a.Speed + b.Speed,
            Accuracy = a.Accuracy + b.Accuracy,
            Evasion = a.Evasion + b.Evasion
        };
    }

    public static StatBlock operator *(StatBlock a, float mutilplier) {
        return new StatBlock {
            MaxHP = Mathf.RoundToInt(a.MaxHP * mutilplier),
            MaxSP = Mathf.RoundToInt(a.MaxSP * mutilplier),
            PAtk = Mathf.RoundToInt(a.PAtk * mutilplier),
            PDef = Mathf.RoundToInt(a.PDef * mutilplier),
            MAtk = Mathf.RoundToInt(a.MAtk * mutilplier),
            MDef = Mathf.RoundToInt(a.MDef * mutilplier),
            Speed = Mathf.RoundToInt(a.Speed * mutilplier),
            Accuracy = Mathf.RoundToInt(a.Accuracy * mutilplier),
            Evasion = Mathf.RoundToInt(a.Evasion * mutilplier),
        };
    }
}


