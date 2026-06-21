


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
}
