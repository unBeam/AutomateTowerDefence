using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dialogues.Configs
{
    [Serializable]
    public class LocalizedStringEntry
    {
        public string LanguageCode;
        [TextArea(3, 10)] public string Text;
    }

    [Serializable]
    public class DialogueChoiceAsset
    {
        public string NodeId;
        public string NextId;
        public string MissionId;
        public List<LocalizedStringEntry> LocalizedTexts = new List<LocalizedStringEntry>();

        public string GetText(string lang)
        {
            for (int i = 0; i < LocalizedTexts.Count; i++)
                if (LocalizedTexts[i].LanguageCode == lang) return LocalizedTexts[i].Text;
            return string.Empty;
        }
    }

    [CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogues/Dialogue Node")]
    public class DialogueNodeAsset : ScriptableObject
    {
        [field: SerializeField] public string ScriptKey { get; private set; }
        [field: SerializeField] public string NodeId { get; private set; }
        [field: SerializeField] public DialogCharacter Character { get; private set; }
        [field: SerializeField] public CharacterEmotion Emotion { get; private set; }
        [field: SerializeField] public string NextId { get; private set; }
        [field: SerializeField] public bool IsChoicePoint { get; private set; }
        [field: SerializeField] public List<DialogueChoiceAsset> Choices { get; private set; } = new List<DialogueChoiceAsset>();
        [field: SerializeField] public string MissionId { get; private set; }
        [field: SerializeField] public string Callback { get; private set; }
        [SerializeField] private List<LocalizedStringEntry> _localizedTexts = new List<LocalizedStringEntry>();

        public string GetText(string lang)
        {
            for (int i = 0; i < _localizedTexts.Count; i++)
                if (_localizedTexts[i].LanguageCode == lang) return _localizedTexts[i].Text;
            return string.Empty;
        }
    }
}
