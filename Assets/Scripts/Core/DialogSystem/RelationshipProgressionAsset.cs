using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogues.Configs
{
    [Serializable]
    public class RelationshipLevelEntry
    {
        public int Level;
        public int RequiredRP;
    }

    [CreateAssetMenu(fileName = "RelationshipProgression", menuName = "Dialogues/Relationship Progression")]
    public class RelationshipProgressionAsset : ScriptableObject
    {
        public List<RelationshipLevelEntry> Levels = new List<RelationshipLevelEntry>();

        public int GetRequiredRP(int level)
        {
            int best = 0;
            for (int i = 0; i < Levels.Count; i++)
                if (Levels[i].Level == level) return Levels[i].RequiredRP;
                else if (Levels[i].Level < level && Levels[i].RequiredRP > best) best = Levels[i].RequiredRP;
            if (best > 0) return best;
            return Levels.Count > 0 ? Levels[Levels.Count - 1].RequiredRP : 100;
        }
    }
}