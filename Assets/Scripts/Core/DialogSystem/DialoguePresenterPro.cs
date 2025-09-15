using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dialogues.Configs;
using Dialogues.Domain;
using Dialogues.UI;

namespace Dialogues.Runtime
{
    public class DialoguePresenterPro
    {
        private readonly IDialogueRepository _repo;
        private readonly ILocalizationService _loc;
        private readonly IRelationshipService _rel;
        private readonly IScriptSettingsRepository _settings;
        private readonly ICallbackDispatcher _callbacks;
        private readonly DialogViewPro _view;

        private DialogueNodeAsset _current;
        private bool _typing;
        private CancellationTokenSource _cts;

        public DialoguePresenterPro(IDialogueRepository repo, ILocalizationService loc, IRelationshipService rel, IScriptSettingsRepository settings, ICallbackDispatcher callbacks, DialogViewPro view)
        {
            _repo = repo;
            _loc = loc;
            _rel = rel;
            _settings = settings;
            _callbacks = callbacks;
            _view = view;
        }

        public bool CanStart(string scriptKey, string characterName)
        {
            ScriptSettingEntry s = _settings.Get(scriptKey);
            if (s == null) return true;
            int level = _rel.GetLevel(characterName);
            return level >= s.RLUnlock;
        }

        public void Start(string scriptKey, string startNodeId)
        {
            DialogueNodeAsset node = string.IsNullOrEmpty(startNodeId) ? _repo.GetStartNode(scriptKey) : _repo.GetNode(scriptKey, startNodeId);
            if (node == null) return;
            _current = node;
            _view.Show();
            BindControls();
            ShowCurrent().Forget();
        }

        private void BindControls()
        {
            _view.SetNextHandler(() =>
            {
                if (_typing)
                {
                    CancelTyping();
                    if (_current != null) _view.SetText(_current.GetLocalizedText(_loc.CurrentLanguage));
                }
                else
                {
                    MoveNext().Forget();
                }
            });
        }

        private async UniTask ShowCurrent()
        {
            if (_current == null)
            {
                _view.Hide();
                return;
            }

            if (_current.Character != null)
            {
                _view.SetName(_current.Character.GetLocalizedName(_loc.CurrentLanguage));
                _view.SetPortrait(_current.Character.GetEmotionSprite(_current.Emotion));
                float p = _rel.GetProgress01(_current.Character.CharacterName);
                _view.SetRelationship(p, string.Empty);
            }

            CancelTyping();
            _cts = new CancellationTokenSource();
            string full = ResolveText(_current);
            _view.HideChoices();
            await TypeText(full, _cts.Token);

            if (!string.IsNullOrEmpty(_current.Callback))
                await _callbacks.DispatchAsync(_current.Callback);

            if (_current.IsChoicePoint && _current.Choices != null && _current.Choices.Count > 0)
            {
                List<(string, System.Action)> items = new List<(string, System.Action)>();
                for (int i = 0; i < _current.Choices.Count; i++)
                {
                    DialogueChoiceAsset ch = _current.Choices[i];
                    string t = ch.GetLocalizedText(_loc.CurrentLanguage);
                    System.Action a = () =>
                    {
                        if (!string.IsNullOrEmpty(ch.MissionId)) { }
                        _current = _repo.GetNode(_current.ScriptKey, ch.NextId);
                        ShowCurrent().Forget();
                    };
                    items.Add((t, a));
                }
                _view.ShowChoices(items);
            }
        }

        private async UniTask MoveNext()
        {
            if (_current == null) return;
            if (!string.IsNullOrEmpty(_current.NextId))
            {
                _current = _repo.GetNode(_current.ScriptKey, _current.NextId);
                await ShowCurrent();
                return;
            }

            AwardOnFinish();
            _view.Hide();
            _current = null;
        }

        private void AwardOnFinish()
        {
            if (_current == null) return;
            ScriptSettingEntry s = _settings.Get(_current.ScriptKey);
            if (s == null) return;
            if (_current.Character == null) return;
            _rel.AddRP(_current.Character.CharacterName, s.RPReward);
        }

        private async UniTask TypeText(string full, CancellationToken token)
        {
            _typing = true;
            int visible = 0;
            int total = CountVisible(full);
            StringBuilder sb = new StringBuilder();

            bool inside = false;
            for (int i = 0; i < full.Length; i++)
            {
                if (token.IsCancellationRequested) break;
                char c = full[i];
                sb.Append(c);
                if (c == '<') inside = true;
                else if (c == '>') inside = false;
                else if (!inside) visible += 1;
                _view.SetText(sb.ToString());
                if (visible >= total) break;
                await UniTask.Delay(1, cancellationToken: token);
            }
            _typing = false;
        }

        private int CountVisible(string s)
        {
            int count = 0;
            bool inside = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '<') { inside = true; continue; }
                if (c == '>') { inside = false; continue; }
                if (!inside) count += 1;
            }
            return count;
        }

        private void CancelTyping()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            _typing = false;
        }

        private string ResolveText(DialogueNodeAsset node)
        {
            string raw = node.GetLocalizedText(_loc.CurrentLanguage);
            return raw.Replace("(PLAYER_NAME)", string.Empty);
        }
    }
}
