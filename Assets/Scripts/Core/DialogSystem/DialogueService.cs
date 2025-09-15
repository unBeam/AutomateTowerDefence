using Dialogues.Configs;
using UniRx;

namespace Dialogues.Runtime
{
    public class DialogueService : IDialogueService
    {
        private readonly IDialogueRepository _repo;
        private readonly ReactiveProperty<DialogueNodeAsset> _current = new ReactiveProperty<DialogueNodeAsset>();

        public IReadOnlyReactiveProperty<DialogueNodeAsset> Current => _current;

        public DialogueService(IDialogueRepository repo)
        {
            _repo = repo;
        }

        public void Start(string scriptKey)
        {
            DialogueNodeAsset node = _repo.GetStartNode(scriptKey);
            _current.Value = node;
        }

        public void Next()
        {
            DialogueNodeAsset node = _current.Value;
            if (node == null) return;
            if (string.IsNullOrEmpty(node.NextId))
            {
                _current.Value = null;
                return;
            }
            DialogueNodeAsset next = _repo.GetNode(node.ScriptKey, node.NextId);
            _current.Value = next;
        }

        public void Choose(int index)
        {
            DialogueNodeAsset node = _current.Value;
            if (node == null) return;
            if (node.Choices == null) return;
            if (index < 0) return;
            if (index >= node.Choices.Count) return;
            DialogueChoiceAsset choice = node.Choices[index];
            if (string.IsNullOrEmpty(choice.NextId))
            {
                _current.Value = null;
                return;
            }
            DialogueNodeAsset next = _repo.GetNode(node.ScriptKey, choice.NextId);
            _current.Value = next;
        }
    }
}