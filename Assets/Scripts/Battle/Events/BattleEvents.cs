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

