using System.Collections.Generic;
using DefaultNamespace.Quests;
using UniRx;
using UnityEngine;
using Zenject;

public class QuestManager : IQuestStarter
{
    private readonly Dictionary<string, QuestInstance> _running = new();
    private readonly QuestCatalog _catalog;
    private readonly QuestEventBus _eventBus;
    private readonly DiContainer _container;
    
    public QuestManager(QuestCatalog catalog, QuestEventBus eventBus, 
        DiContainer container)
    {
        _catalog = catalog;
        _eventBus = eventBus;
        _container = container;
    }

    public void StartQuestById(string id)
    {
        if (_running.ContainsKey(id))
            return;

        QuestDefinitionAsset def = _catalog.GetById(id);
        if (def == null)
        {
            return;
        }
        
        if (def.StartHandler != null)
        {
            def.StartHandler.InjectDependencies(_container);
            def.StartHandler.OnQuestStarted();
        }
        
        Debug.Log($"quest {id} started");
        IFailCondition fail = def.HasTimeLimit ? new TimerFailCondition(def.TimeLimitSeconds) : null;

        QuestModel model = new QuestModel(def.Condition, def.Reward, this, _eventBus);
        QuestInstance quest = new QuestInstance(def.Id, model, fail);

        _running[def.Id] = quest;

        quest.IsCompleted
            .Where(x => x)
            .Take(1)
            .Subscribe(_ =>
            {
                _running.Remove(def.Id);
                Debug.Log($"quest {id} finished");
            });

        quest.IsFailed
            .Where(x => x)
            .Take(1)
            .Subscribe(_ =>
            {
                _running.Remove(def.Id);
            });
    }

}