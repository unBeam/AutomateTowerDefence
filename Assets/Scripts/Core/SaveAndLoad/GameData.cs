using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class GameData
{
    public int Money;
    public int Energy = 5;
    
    public List<RelationshipData> Relationships = new();
    [NonSerialized] public Dictionary<string, RelationshipProgress> RelationshipsProgress = new();
    
    public void ProcessLoadedData()
    {
        foreach (RelationshipData relationship in Relationships)
        {
            RelationshipsProgress.Add(relationship.CharacterId, relationship.Progress);
        }
    }

    public void PrepareDataForSave()
    {
        Relationships = RelationshipsProgress
            .Select(kvp => new RelationshipData { CharacterId = kvp.Key, Progress = kvp.Value })
            .ToList();
    }
}