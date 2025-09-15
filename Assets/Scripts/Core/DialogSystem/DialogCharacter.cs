using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogues.Configs
{
    [CreateAssetMenu(fileName = "DialogCharacter", menuName = "Dialogues/Character")]
    public class DialogCharacter : ScriptableObject
    {
        [SerializeField] private string _characterName;
        [SerializeField] private Sprite _defaultEmotion;
        [SerializeField] private Sprite _happyEmotion;
        [SerializeField] private Sprite _sadEmotion;
        [SerializeField] private Sprite _thinkingEmotion;
        [SerializeField] private Sprite _neutralEmotion;
        [SerializeField] private Sprite _angryEmotion;
        [SerializeField] private Sprite _surprisedEmotion;

        [SerializeField] private List<LocalizedNameEntry> _localizedNames = new List<LocalizedNameEntry>();
        private Dictionary<string, string> _runtimeMap;

        public string CharacterName => _characterName;

        public string GetLocalizedName(string lang)
        {
            if (_runtimeMap == null) BuildRuntimeMap();
            string key = lang.ToLowerInvariant();
            if (_runtimeMap.TryGetValue(key, out string value)) return value;
            return CharacterName;
        }

        public void SetLocalization(Dictionary<string, string> map)
        {
            _localizedNames = new List<LocalizedNameEntry>();
            foreach (var pair in map)
            {
                _localizedNames.Add(new LocalizedNameEntry { Language = pair.Key, LocalizedValue = pair.Value });
            }
            BuildRuntimeMap();
        }

        private void BuildRuntimeMap()
        {
            _runtimeMap = new Dictionary<string, string>();
            foreach (var entry in _localizedNames)
            {
                if (!string.IsNullOrWhiteSpace(entry.Language))
                    _runtimeMap[entry.Language.ToLowerInvariant()] = entry.LocalizedValue;
            }
        }

        public Sprite GetEmotionSprite(CharacterEmotion emotion)
        {
            switch (emotion)
            {
                case CharacterEmotion.Happy: return _happyEmotion;
                case CharacterEmotion.Sad: return _sadEmotion;
                case CharacterEmotion.Thinking: return _thinkingEmotion;
                case CharacterEmotion.Neutral: return _neutralEmotion;
                case CharacterEmotion.Angry: return _angryEmotion;
                case CharacterEmotion.Surprised: return _surprisedEmotion;
                default: return _defaultEmotion;
            }
        }
    }

    public enum CharacterEmotion
    {
        Default = 0,
        Happy = 1,
        Sad = 2,
        Thinking = 3,
        Neutral = 4,
        Angry = 5,
        Surprised = 6
    }

    [Serializable]
    public class LocalizedNameEntry
    {
        public string Language;
        public string LocalizedValue;
    }
}
