using Dialogues.Runtime;
using Zenject;

namespace Dialogues.App
{
    public class DialogueService
    {
        private readonly IDialogueRepository _repo;
        private readonly ILocalizationService _loc;
        private readonly IRelationshipService _rel;
        private readonly IScriptSettingsRepository _settings;
        private readonly ICallbackDispatcher _callbacks;
        private readonly DiContainer _di;

        public DialogueService(IDialogueRepository repo, ILocalizationService loc, IRelationshipService rel, IScriptSettingsRepository settings, ICallbackDispatcher callbacks, DiContainer di)
        {
            _repo = repo;
            _loc = loc;
            _rel = rel;
            _settings = settings;
            _callbacks = callbacks;
            _di = di;
        }

        public DialoguePresenterPro CreatePresenter(Dialogues.UI.DialogViewPro view)
        {
            DialoguePresenterPro p = _di.Instantiate<DialoguePresenterPro>(new object[] { _repo, _loc, _rel, _settings, _callbacks, view });
            return p;
        }
    }
}