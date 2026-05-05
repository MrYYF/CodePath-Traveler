public enum GameMode
{
    Explore,
    InteractionMenu,
    Battle,
    Pause
}

public enum ActiveInputActionMap
{
    Player,
    UI
}

public enum CameraView {
    Explore,
    Battle,
    BattleResult
}

public enum Job {
    Any,
    None,
    Warrior, //战士
    Cleric, //神官
    Mage, //法师
    Archer, //弓箭手
    Thief //盗贼
}

public enum GrowthRank {
    S,
    A,
    B,
    C,
    D
}

public enum ItemType {
    None,
    Equipment = 0, // 装备
    Consumable = 1, // 消耗品
    Accessory, // 饰品
    Material // 材料
}

public enum ItemIconKey {
    // 物品类型枚举
    Weapon, // 武器
    Armor, // 防具
    Accessory, // 饰品
    Healing, // 治疗
    SP, // SP恢复
    Revive, // 复活
    Cure, // 状态异常治疗
    KeyItem, // 关键道具
}

public enum PanelType {
    Item,
    Sell,
    Buy,
    Equipment,
}

public enum EquipmentCategory {
    Weapon,
    Shield,
    Head,
    Body,
    Accessory
}

public enum EquipSlot {
    None,
    Weapon,
    Shield,
    Head,
    Body,
    Accessory1,
    Accessory2
}

public enum WeaponType {
    None = 0,
    Sword,
    Dagger,
    Axe,
    Spear,
    Bow,
    Staff
}