
using System;

[CreateAssetMenu(menuName = "Character/Character/Ally", order = 1)]
public class AllyDefinitionSO : CharacterDefinitionSO
{
    [Header("Ally Specific")]
    public GlobalGrowthConfigSO globalGrowthConfigSO;
    public GrowthProfile growthProfile;

    [Header("Equipment Capability")]
    public List<WeaponType> EquipableWeaponTypes = new();

    [Header("Initial Equipment")]
    public List<InitialEquipmentEntry> InitialEquipment = new();

    [Serializable]
    public struct InitialEquipmentEntry {
        public EquipSlot equipSlot;
        public EquipmentItemSO equiptmentItem;
    }

    public bool CanEquipWeaponType(WeaponType weaponType) {
        if(weaponType == WeaponType.None) return false;
        return EquipableWeaponTypes.Contains(weaponType);
    }

    #region  Ù–‘≥…≥§
    public StatBlock GetStatForLevel(int level) {
        float hpMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.HP).Evaluate(level);
        float spMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.SP).Evaluate(level);
        float patkMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.Patk).Evaluate(level);
        float pdefMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.Pdef).Evaluate(level);
        float matkMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.Matk).Evaluate(level);
        float mdefMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.Mdef).Evaluate(level);
        float speedMult = globalGrowthConfigSO.GetGrowthByRank(growthProfile.Speed).Evaluate(level);

        return new StatBlock {
            MaxHP = Mathf.RoundToInt(BaseStats.MaxHP * hpMult),
            MaxSP = Mathf.RoundToInt(BaseStats.MaxSP * spMult),
            PAtk = Mathf.RoundToInt(BaseStats.PAtk * patkMult),
            PDef = Mathf.RoundToInt(BaseStats.PDef * pdefMult),
            MAtk = Mathf.RoundToInt(BaseStats.MAtk * matkMult),
            MDef = Mathf.RoundToInt(BaseStats.MDef * mdefMult),
            Speed = Mathf.RoundToInt(BaseStats.Speed * speedMult),
            Accuracy = BaseStats.Accuracy,
            Evasion = BaseStats.Evasion
        };

    }
    #endregion

}

[Serializable]
public struct GrowthProfile
{
    public GrowthRank HP;
    public GrowthRank SP;
    public GrowthRank Patk;
    public GrowthRank Pdef;
    public GrowthRank Matk;
    public GrowthRank Mdef;
    public GrowthRank Speed;
}
