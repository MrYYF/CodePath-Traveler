using System;

[Serializable]
public class CharacterRuntimeData 
{
    public CharacterDefinitionSO Definition;

    public int Level;
    public int CurrentHP;
    public int CurrentSP;
    public int CurrentBP;
    public int CurrentExp;
    public string DisplayName => Definition.Name;
    public StatBlock EquipmentStats;

    public CharacterRuntimeData(CharacterDefinitionSO definition) {
        Definition = definition;
        EquipmentStats = StatBlock.Zero;

        var stats = GetBaseStats();
        CurrentHP = stats.MaxHP;
        CurrentSP = stats.MaxSP;
        CurrentBP = 0;
    }

    public StatBlock GetBaseStats() {
        if (Definition is AllyDefinitionSO allyDefinition)
            return allyDefinition.GetStatForLevel(Level);

        if (Definition is EnemyDefinitionSO enemyDefinition)
            return enemyDefinition.BaseStats;

        return Definition != null ? Definition.BaseStats : StatBlock.Zero;
    }

    public StatBlock GetTotalStats() => GetBaseStats() + EquipmentStats;


    #region 数据变化接口
    public void HealFull() {
        var stats = GetTotalStats();
        CurrentHP = stats.MaxHP;
        CurrentSP = stats.MaxSP;
    }

    public void ModifyHP(int amount) {
        var stats = GetTotalStats();
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, stats.MaxHP);
    }

    public void ModifySP(int amount) {
        var stats = GetTotalStats();
        CurrentSP = Mathf.Clamp(CurrentSP + amount, 0, stats.MaxSP);
    }

    public void ResetBattleBP() {
        CurrentBP = 0;
    }

    #endregion
}
