using System;
using DefaultNamespace.Quests;
using UniRx;

public class QuestEventBus
{
    private readonly Subject<IQuestEvent> _events = new();
    public IObservable<IQuestEvent> Events => _events;

    public void Publish(IQuestEvent questEvent)
    {
        _events.OnNext(questEvent);
    }
}