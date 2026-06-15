

/// <summary>
/// 技能
/// </summary>
[CreateAssetMenu(menuName = "Battle/Skill")]
public class SkillDataSO : ScriptableObject
{
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
}
