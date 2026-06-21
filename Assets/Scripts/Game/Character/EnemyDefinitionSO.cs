


[CreateAssetMenu(menuName = "Character/Enemy", order = 1)]
public class EnemyDefinitionSO : CharacterDefinitionSO
{
    [Header("Rewards")]
    public int ExpReward;
    public int MoneyReward;

    public List<InventoryItem> Drops;
}
