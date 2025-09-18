using System;
using System.Collections.Generic;
using DefaultNamespace.Quests;
using UniRx;

public class QuestModel : IDisposable
{
    private readonly QuestCondition _condition;
    private readonly QuestReward _reward;
    private readonly IQuestStarter _questStarter;
    private readonly QuestEventBus _eventBus;

    private readonly Dictionary<string, int> _progress = new();
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<bool> IsCompleted { get; } = new(false);

    public QuestModel(
        QuestCondition condition,
        QuestReward reward,
        IQuestStarter questStarter,
        QuestEventBus eventBus)
    {
        _condition = condition;
        _reward = reward;
        _questStarter = questStarter;
        _eventBus = eventBus;

        Subscribe();
    }

    private void Subscribe()
    {
        _eventBus.Events.Subscribe(OnEvent).AddTo(_disposables);
    }

    private void OnEvent(IQuestEvent questEvent)
    {
        foreach (var requirement in _condition.Requirements)
        {
            if (requirement.EventKey != questEvent.Key)
                continue;

            if (!_progress.ContainsKey(requirement.EventKey))
                _progress[requirement.EventKey] = 0;

            _progress[requirement.EventKey] += questEvent.Amount;
        }
        
        bool allMet = _condition.Requirements.TrueForAll(req =>
            _progress.TryGetValue(req.EventKey, out int value) &&
            value >= req.RequiredAmount);

        if (allMet)
            Complete();
    }

    private void Complete()
    {
        if (IsCompleted.Value)
            return;

        IsCompleted.Value = true;
        _reward?.Give(_questStarter);
        Dispose();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
