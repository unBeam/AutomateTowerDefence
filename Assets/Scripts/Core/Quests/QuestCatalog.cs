using System.Collections.Generic;
using DefaultNamespace.Quests;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest Catalog")]
public class QuestCatalog : ScriptableObject
{
    public List<QuestDefinitionAsset> Definitions = new();
    
    public QuestDefinitionAsset GetById(string id)
    {
        return Definitions.Find(q => q.Id == id);
    }
}