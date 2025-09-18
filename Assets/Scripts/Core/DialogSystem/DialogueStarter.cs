using System.Collections.Generic;
using Dialogues.Configs;
using Dialogues.Runtime;
using Dialogues.UI;
using UniRx;
using UnityEngine;
using Zenject;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogViewPro _view;
    [SerializeField] private string _scriptKey;

    [Inject] private IDialogueService _service;
    [Inject] private ILocalizationService _loc;

    private System.IDisposable _sub;

    private void Start()
    {
        _service.Start(_scriptKey);
        _sub = _service.Current.Subscribe(OnNodeChanged);
    }

    private void OnDestroy()
    {
        if (_sub != null) _sub.Dispose();
    }

    private void OnNodeChanged(DialogueNodeAsset node)
    {
        if (node == null)
        {
            _view.Hide();
        }
        else
        {
            _view.Show();
            _view.SetName(node.Character != null ? node.Character.GetLocalizedName(_loc.CurrentLanguage) : "???");
            _view.SetPortrait(node.Character != null ? node.Character.GetEmotionSprite(node.Emotion) : null);
            string text = node.GetLocalizedText(_loc.CurrentLanguage);
            _view.SetText(text);
            if (node.IsChoicePoint && node.Choices.Count > 0)
            {
                List<(string, System.Action)> items = new List<(string, System.Action)>();
                for (int i = 0; i < node.Choices.Count; i++)
                {
                    DialogueChoiceAsset choice = node.Choices[i];
                    string choiceText = choice.GetLocalizedText(_loc.CurrentLanguage);
                    int idx = i;
                    items.Add((choiceText, () => _service.Choose(idx)));
                }
                _view.ShowChoices(items);
            }
            else
            {
                _view.HideChoices();
                _view.SetNextHandler(() => _service.Next());
            }
        }
    }
}