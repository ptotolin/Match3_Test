using System;

public interface IEventBus
{
    void Publish<T>(T eventData) where T : ISpecialAbilityEventData;
    void Subscribe<T>(Action<T> handler) where T : ISpecialAbilityEventData;
    void Unsubscribe<T>(Action<T> handler) where T : ISpecialAbilityEventData;
}