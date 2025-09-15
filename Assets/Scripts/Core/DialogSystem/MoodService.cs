using System.Collections.Generic;

namespace Dialogues.Runtime
{
    public class MoodService : IMoodService
    {
        private readonly Dictionary<string, string> _moods = new Dictionary<string, string>();

        public string GetMood(string characterName)
        {
            if (_moods.TryGetValue(characterName, out string m)) return m;
            return string.Empty;
        }

        public void SetMood(string characterName, string mood)
        {
            _moods[characterName] = mood;
        }
    }
}