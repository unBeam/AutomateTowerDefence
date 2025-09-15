using Dialogues.App;
using Dialogues.Configs;
using Dialogues.Runtime;
using Zenject;
using UnityEngine;

namespace Dialogues.Installers
{
    [CreateAssetMenu(fileName = "DialogueProjectInstaller", menuName = "Installers/Dialogue Project Installer")]
    public class DialogueProjectInstaller : ScriptableObjectInstaller<DialogueProjectInstaller>
    {
        public ScriptSettingsAsset ScriptSettings;
        public RelationshipProgressionAsset RelationshipProgression;
        public GiftCatalogAsset GiftCatalog;

        public override void InstallBindings()
        {
            Container.Bind<IDialogueRepository>().To<ResourceDialogueRepository>().AsSingle();
            Container.Bind<IScriptSettingsRepository>().To<ScriptSettingsRepository>().AsSingle().WithArguments(ScriptSettings);
            Container.Bind<IRelationshipService>().To<RelationshipService>().AsSingle().WithArguments(RelationshipProgression);
            Container.Bind<IGiftService>().To<GiftService>().AsSingle().WithArguments(GiftCatalog, Container.Resolve<IRelationshipService>());
            Container.Bind<IMoodService>().To<MoodService>().AsSingle();
            Container.Bind<ICallbackDispatcher>().To<CallbackDispatcher>().AsSingle();
            Container.Bind<DialogueService>().AsSingle();
        }
    }
}