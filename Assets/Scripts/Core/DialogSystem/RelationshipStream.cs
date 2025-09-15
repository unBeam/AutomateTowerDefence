using System;
using System.Collections.Generic;
using Dialogues.Configs;
using UniRx;

namespace Dialogues.Runtime
{
    public class RelationshipStream : IObservableRelationshipStream
    {
        private readonly Subject<RelationshipChangedEvent> _changed = new Subject<RelationshipChangedEvent>();
        private readonly Subject<RelationshipLevelUpEvent> _levelUp = new Subject<RelationshipLevelUpEvent>();

        public IObservable<RelationshipChangedEvent> Changed => _changed;
        public IObservable<RelationshipLevelUpEvent> LevelUp => _levelUp;

        public void PublishChanged(string character, int level, int rp)
        {
            RelationshipChangedEvent e = new RelationshipChangedEvent { CharacterName = character, Level = level, RP = rp };
            _changed.OnNext(e);
        }

        public void PublishLevelUp(string character, int newLevel)
        {
            RelationshipLevelUpEvent e = new RelationshipLevelUpEvent { CharacterName = character, NewLevel = newLevel };
            _levelUp.OnNext(e);
        }
    }

    public class RelationshipService : IRelationshipService
    {
        private readonly RelationshipProgressionAsset _progression;
        private readonly Dictionary<string, int> _levels = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _rp = new Dictionary<string, int>();
        private readonly RelationshipStream _stream = new RelationshipStream();

        public RelationshipService(RelationshipProgressionAsset progression)
        {
            _progression = progression;
        }

        public IObservableRelationshipStream Stream => _stream;

        public int GetLevel(string characterName)
        {
            if (_levels.TryGetValue(characterName, out int level)) return level;
            return 0;
        }

        public int GetRP(string characterName)
        {
            if (_rp.TryGetValue(characterName, out int points)) return points;
            return 0;
        }

        public float GetProgress01(string characterName)
        {
            int level = GetLevel(characterName);
            int need = _progression != null ? _progression.GetRequiredRP(level + 1) : 100;
            int cur = GetRP(characterName);
            if (need <= 0) return 0f;
            float v = (float)cur / (float)need;
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        public void AddRP(string characterName, int amount)
        {
            int currentRP = GetRP(characterName) + amount;
            int currentLevel = GetLevel(characterName);
            int need = _progression != null ? _progression.GetRequiredRP(currentLevel + 1) : 100;
            while (currentRP >= need && need > 0)
            {
                currentRP -= need;
                currentLevel += 1;
                _stream.PublishLevelUp(characterName, currentLevel);
                need = _progression != null ? _progression.GetRequiredRP(currentLevel + 1) : 100;
            }
            _rp[characterName] = currentRP;
            _levels[characterName] = currentLevel;
            _stream.PublishChanged(characterName, currentLevel, currentRP);
        }
    }
}
