using Dialogues.Configs;
using Dialogues.Runtime;
using UnityEngine;
using Zenject;

namespace Dialogues.Installers
{
    public class DialogueSceneInstaller : MonoInstaller
    {
        [SerializeField] private ScriptSettingsAsset _scriptSettings;
        [SerializeField] private RelationshipProgressionAsset _relationshipProgression;
        [SerializeField] private GiftCatalogAsset _giftCatalog;

        public override void InstallBindings()
        {
            Container.Bind<ScriptSettingsAsset>().FromInstance(_scriptSettings).AsSingle();
            Container.Bind<RelationshipProgressionAsset>().FromInstance(_relationshipProgression).AsSingle();
            Container.Bind<GiftCatalogAsset>().FromInstance(_giftCatalog).AsSingle();

            Container.Bind<IDialogueRepository>().To<ResourceDialogueRepository>().AsSingle();
            Container.Bind<IScriptSettingsRepository>().To<ScriptSettingsRepository>().AsSingle();
            Container.Bind<IRelationshipService>().To<RelationshipService>().AsSingle();
            Container.Bind<IGiftService>().To<GiftService>().AsSingle();
            Container.Bind<IMoodService>().To<MoodService>().AsSingle();
            Container.Bind<ICallbackDispatcher>().To<CallbackDispatcher>().AsSingle();

            Container.Bind<LocalizationData>().AsSingle().WithArguments("ru");
            Container.Bind<ILocalizationService>().To<LocalizationDataAdapter>().AsSingle();

            Container.Bind<IDialogueService>().To<DialogueService>().AsSingle();
        }
    }
}