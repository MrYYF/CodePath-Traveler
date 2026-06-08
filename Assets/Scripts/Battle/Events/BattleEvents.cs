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

