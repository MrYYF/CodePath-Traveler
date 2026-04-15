using Framework.Event;

/// <summary>
/// 交互状态变化事件，用于通知UI更新当前交互对象和可用操作列表
/// </summary>
public readonly struct InteractionChangedEvent : IEvent
{
    public readonly InteractionBase target;

    public readonly bool inRange;

    public InteractionChangedEvent(InteractionBase target, bool inRange) {
        this.target = target;
        this.inRange = inRange;
    }

}
