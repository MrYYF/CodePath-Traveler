using System;
using Framework.Event;

/// <summary>
/// 事件总线，负责事件的订阅、发布、取消订阅
/// </summary>
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

        // 尝试获取已订阅的接收者并确保非空
        if (EventDic.TryGetValue(eventType, out var receivers) && receivers != null) {
            // 倒序遍历可以在遍历时安全地移除元素
            for (int i = receivers.Count - 1; i >= 0; i--) {
                var receiver = receivers[i];
                if (receiver is UnityEngine.Object unityObj && unityObj == null) {
                    // 移除已被 Unity 销毁的目标
                    receivers.RemoveAt(i);
                    continue;
                }

                ((IEventReceiver<TEvent>)receiver).OnEvent(evt);
            }

            if (receivers.Count == 0) {
                EventDic.Remove(eventType);
            }
        }
    }
}
