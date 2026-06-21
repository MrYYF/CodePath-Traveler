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

public readonly struct SkillNameDisplayEvent : IEvent {
    public readonly BattleEntity Actor;
    public readonly string SkillName;

    public SkillNameDisplayEvent(BattleEntity actor, string skillName) {
        Actor = actor;
        SkillName = skillName;
    }
}

public readonly struct BattleNotificationEvent : IEvent {
    public readonly string Message;
    public readonly bool IsSuccess;

    public BattleNotificationEvent(string message, bool isSuccess = false) {
        Message = message;
        IsSuccess = isSuccess;
    }
}

