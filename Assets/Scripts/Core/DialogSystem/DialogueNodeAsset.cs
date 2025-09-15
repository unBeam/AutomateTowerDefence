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

        public string GetLocalizedText(string langCode)
        {
            if (LocalizedTexts == null || LocalizedTexts.Count == 0)
                return "[missing choice]";
            LocalizedStringEntry entry = LocalizedTexts.FirstOrDefault(t => t.LanguageCode == langCode);
            return entry != null ? entry.Text : "[missing choice]";
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

        public string GetLocalizedText(string langCode)
        {
            if (_localizedTexts == null || _localizedTexts.Count == 0)
                return "[missing text]";
            LocalizedStringEntry entry = _localizedTexts.FirstOrDefault(t => t.LanguageCode == langCode);
            return entry != null ? entry.Text : "[missing text]";
        }
    }
}
