/// <summary>
/// 游戏模式
/// </summary>
public enum GameMode {
    Explore,
    InteractionMenu,
    Battle,
    Pause
}

/// <summary>
/// 激活的输入映射
/// </summary>
public enum ActiveInputActionMap {
    Player,
    UI
}

/// <summary>
/// 相机视角
/// </summary>
public enum CameraView {
    Explore,
    Battle,
    BattleResult
}

/// <summary>
/// 职业
/// </summary>
public enum Job {
    Any,
    None,
    Warrior, //战士
    Cleric, //神官
    Mage, //法师
    Archer, //弓箭手
    Thief //盗贼
}

/// <summary>
/// 成长等级
/// </summary>
public enum GrowthRank {
    S,
    A,
    B,
    C,
    D
}

/// <summary>
/// 物品类型
/// </summary>
public enum ItemType {
    None,
    Equipment = 0, // 装备
    Consumable = 1, // 消耗品
    Accessory, // 饰品
    Material // 材料
}

/// <summary>
/// 物品类型图标
/// </summary>
public enum ItemIconKey {
    Weapon, // 武器
    Armor, // 防具
    Accessory, // 饰品
    Healing, // 治疗
    SP, // SP恢复
    Revive, // 复活
    Cure, // 状态异常治疗
    KeyItem, // 关键道具
}

/// <summary>
/// 面板类型
/// </summary>
public enum PanelType {
    Item,
    Sell,
    Buy,
    Equipment,
}

/// <summary>
/// 装备类别
/// </summary>
public enum EquipmentCategory {
    Weapon,
    Shield,
    Head,
    Body,
    Accessory
}

/// <summary>
/// 装备槽位
/// </summary>
public enum EquipSlot {
    None,
    Dagger,
    Sword,
    Spear,
    Axe,
    Bow,
    Staff,
    Shield,
    Head,
    Body,
    Accessory1,
    Accessory2
}

/// <summary>
/// 武器类型
/// </summary>
public enum WeaponType {
    None = 0,
    Dagger,
    Sword,
    Spear,
    Axe,
    Bow,
    Staff
}

/// <summary>
/// 状态类型
/// </summary>
public enum StatType {
    MaxHP = 0,
    MaxSP = 1,
    PAtk = 2,
    PDef = 3,
    MAtk = 4,
    MDef = 5,
    Speed = 6,
    Accuracy = 7,
    Evasion = 8,
    CurrentHP = 100,
    CurrentSP = 101,
    CurrentBP = 102,
}

/// <summary>
/// 敌人排列阵型
/// </summary>
public enum EnemyLayoutFomation {
    Line = 0,
    BossTriangle = 1,
    VShape = 2,
    Circle = 3,
    Random = 4
}

public enum FadeStyle {
    PanelFade,
    WipeMask,
}

public enum BattleCommandType {
    Attack, // 普通攻击
    Skill, // 技能
    Item, // 物品
    Defend, // 防御
    Escape // 逃跑
}

public enum TargetType {
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    AllAllies,
    Self
}

public enum SkillType {
    Damage,
    Heal,
    Buff,
    Debuff,
}

public enum DamageKind {
    Physical,
    Magical,
}

public enum ElementType {
    None,
    Fire,
    Ice,
    Lightning,
    Wind,
    Light,
    Dark
}

public enum DamageType {
    Nomal = 1,
    Heal = 2,
    Magic = 3,
    Gold = 4,
}