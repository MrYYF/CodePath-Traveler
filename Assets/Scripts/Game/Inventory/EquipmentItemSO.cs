

/// <summary>
/// 装备信息so文件，包含了装备的类别、数值
/// </summary>
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
