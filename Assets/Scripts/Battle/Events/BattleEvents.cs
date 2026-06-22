using Framework.Event;

/// <summary>
/// 战斗开始事件
/// </summary>
public readonly struct BattleStartedEvent : IEvent {
    // 可以携带战斗类型等参数用于切换BGM等

    //public readonly BattleStartPreload StartPreload;
    //public BattleStartedEvent(BattleStartPreload startPreload) {
    //    StartPreload = startPreload;
    //}
}

/// <summary>
/// 当前激活的行动者发生变化事件
/// </summary>
public readonly struct ActiveEntityChangedEvent : IEvent {
    public readonly BattleEntity Entity;

    public ActiveEntityChangedEvent(BattleEntity entity) {
        Entity = entity;
    }
}

/// <summary>
/// 实体状态变化事件
/// </summary>
public readonly struct EntityStatChangedEvent : IEvent {
    public readonly BattleEntity Entity;
    public readonly StatType StatType;
    public readonly int NewValue;
    public readonly int MaxValue;

    public EntityStatChangedEvent(BattleEntity entity, StatType statType, int newValue, int maxValue) {
        Entity = entity;
        StatType = statType;
        NewValue = newValue;
        MaxValue = maxValue;
    }
}

// 技能名称展示事件
public readonly struct SkillNameDisplayEvent : IEvent {
    public readonly BattleEntity Actor;
    public readonly string SkillName;

    public SkillNameDisplayEvent(BattleEntity actor, string skillName) {
        Actor = actor;
        SkillName = skillName;
    }
}

// 战斗通知事件
public readonly struct BattleNotificationEvent : IEvent {
    public readonly string Message;
    public readonly bool IsSuccess;

    public BattleNotificationEvent(string message, bool isSuccess = false) {
        Message = message;
        IsSuccess = isSuccess;
    }
}

// 护盾变化事件
public readonly struct EntityShieldChangedEvent : IEvent {
    public readonly BattleEntity Target;
    public readonly int NewShield;

    public EntityShieldChangedEvent(BattleEntity target, int newShield) {
        Target = target;
        NewShield = newShield;
    }
}

// 弱点变化事件
public readonly struct EntityWeaknessChangedEvent : IEvent {
    public readonly BattleEntity Target;

    public EntityWeaknessChangedEvent(BattleEntity target) {
        Target = target;
    }
}

/// <summary>
/// 破盾事件
/// </summary>
public readonly struct EntityBreakEvent : IEvent {
    public readonly BattleEntity Target;

    public EntityBreakEvent(BattleEntity target) {
        Target = target;
    }
}

/// <summary>
/// 破盾恢复事件
/// </summary>
public readonly struct EntityRecoverFromBreakEvent : IEvent {
    public readonly BattleEntity Target;

    public EntityRecoverFromBreakEvent(BattleEntity target) {
        Target = target;
    }
}

