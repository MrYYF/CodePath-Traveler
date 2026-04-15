using System;
using Framework.Event;

public static class EventBus {
    private static readonly Dictionary<Type, List<object>> EventDic = new();

    public static void Subscribe<TEvent>(IEventReceiver<TEvent> receiver) where TEvent : IEvent {
        Type eventType = typeof(TEvent);

        if (!EventDic.TryGetValue(eventType, out var receivers)) {
            receivers = new List<object>();
            EventDic[eventType] = receivers;
        }

        if (!receivers.Contains(receiver)) {
            receivers.Add(receiver);
        }
    }

    public static void Unsubscribe<TEvent>(IEventReceiver<TEvent> receiver) where TEvent : IEvent {
        Type eventType = typeof(TEvent);
        if (EventDic.TryGetValue(eventType, out var receivers)) {
            receivers.Remove(receiver);

            if (receivers.Count == 0) {
                EventDic.Remove(eventType);
            }
        }
    }

    public static void Publish<TEvent>(TEvent evt) where TEvent : IEvent {
        Type eventType = typeof(TEvent);

        // 发布事件
        if (EventDic.TryGetValue(eventType, out var receivers)) {
            foreach (var receiver in receivers) {
                if(receiver is UnityEngine.Object unityObj && unityObj == null) {
                    // 处理Unity对象被销毁的情况
                    receivers.Remove(receiver);
                    continue;
                }
                ((IEventReceiver<TEvent>)receiver).OnEvent(evt);
            }
        }

        if(receivers.Count == 0) {
            EventDic.Remove(eventType);
        }
    }
}
