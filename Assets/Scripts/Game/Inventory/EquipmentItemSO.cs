
[CreateAssetMenu(menuName = "Inventory/Equiptment Item")]
public class EquipmentItemSO : ItemDefinitionSO
{
    [Header("Equipment Config")]
    public EquipmentCategory equipmentCategory = EquipmentCategory.Weapon;

    public WeaponType weaponType = WeaponType.Sword;

    [Header("Stats Bouns")]
    public StatBlock statBouns = StatBlock.Zero;

    private void OnValidate() {
        if(equipmentCategory != EquipmentCategory.Weapon) {
            weaponType = WeaponType.None;
        }
    }
}
