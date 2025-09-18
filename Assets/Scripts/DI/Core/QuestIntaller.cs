using DI;
using UnityEngine;

public class QuestInstaller : BaseBindings
{
    [SerializeField] private QuestCatalog _catalog;
    [SerializeField] private QuestChainCatalog _questChainCatalog;

    public override void InstallBindings()
    {
        BindInstance(_catalog);
        BindInstance(_questChainCatalog);
        BindNewInstance<QuestManager>();
        BindNewInstance<QuestEventBus>();
    }
}