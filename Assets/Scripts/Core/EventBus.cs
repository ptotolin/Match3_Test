using System;
using System.Collections.Generic;

public class EventBus : IEventBus
{
    private Dictionary<Type, Delegate> handlers = new();

    public void Publish<T>(T eventData)
    {
        Type eventType = typeof(T);

        if (handlers.TryGetValue(eventType, out var handler) && handler != null) {
            ((Action<T>)handler).Invoke(eventData);
        }
    }
    
    public void Subscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);

        if (!handlers.ContainsKey(eventType)) {
            handlers[eventType] = null;
        }

        handlers[eventType] = (Action<T>)handlers[eventType] + handler;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);

        if (handlers.ContainsKey(eventType)) {
            handlers[eventType] = (Action<T>)handlers[eventType] - handler;

            if (handlers[eventType] == null) {
                handlers.Remove(eventType);
            }
        }
    }

    public void Clear()
    {
        handlers.Clear();
    }
}