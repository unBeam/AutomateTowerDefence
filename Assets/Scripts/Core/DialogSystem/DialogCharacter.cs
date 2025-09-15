using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Dialogues.Configs
{
    [CreateAssetMenu(fileName = "DialogCharacter", menuName = "Dialogues/Character")]
    public class DialogCharacter : ScriptableObject
    {
        [SerializeField] private string _characterName;
        [SerializeField] private Sprite _defaultEmotion;
        [SerializeField] private List<Sprite> _happyEmotion = new();
        [SerializeField] private List<Sprite> _sadEmotion = new();
        [SerializeField] private List<Sprite> _thinkingEmotion = new();
        [SerializeField] private List<Sprite> _neutralEmotion = new();
        [SerializeField] private List<Sprite> _angryEmotion = new();
        [SerializeField] private List<Sprite> _surprisedEmotion = new();

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
            Random rnd = new Random();
            switch (emotion)
            {
                case CharacterEmotion.Happy: return _happyEmotion[rnd.Next(0, _happyEmotion.Count)];
                case CharacterEmotion.Sad: return _sadEmotion[rnd.Next(0, _sadEmotion.Count)];
                case CharacterEmotion.Thinking: return _thinkingEmotion[rnd.Next(0, _thinkingEmotion.Count)];
                case CharacterEmotion.Neutral: return _neutralEmotion[rnd.Next(0, _neutralEmotion.Count)];
                case CharacterEmotion.Angry: return _angryEmotion[rnd.Next(0,_angryEmotion.Count)];
                case CharacterEmotion.Surprised: return _surprisedEmotion[rnd.Next(0, _surprisedEmotion.Count)];
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
