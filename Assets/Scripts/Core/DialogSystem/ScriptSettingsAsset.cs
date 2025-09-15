using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogues.Configs
{
    [Serializable]
    public class ScriptSettingEntry
    {
        public string CharacterName;
        public string ScriptKey;
        public int RLUnlock;
        public int RLSuccess;
        public int RPReward;
        public string Description;
    }

    [CreateAssetMenu(fileName = "ScriptSettings", menuName = "Dialogues/Script Settings")]
    public class ScriptSettingsAsset : ScriptableObject
    {
        public List<ScriptSettingEntry> Entries = new List<ScriptSettingEntry>();

        public ScriptSettingEntry Get(string scriptKey)
        {
            for (int i = 0; i < Entries.Count; i++)
                if (Entries[i].ScriptKey == scriptKey) return Entries[i];
            return null;
        }
    }
}