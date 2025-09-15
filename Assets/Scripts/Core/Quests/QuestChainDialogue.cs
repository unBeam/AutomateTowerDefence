using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Quests;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest Chain Catalog")]
public class QuestChainCatalog : ScriptableObject
{
    public List<QuestChainConfig> QuestChains = new();

    public QuestDefinitionAsset FindLastCheckpointForQuest(string questId)
    {
        foreach (var chain in QuestChains)
        {
            var questIds = chain.QuestsInOrder.Select(q => q.Id).ToList();
            int questIndex = questIds.IndexOf(questId);

            if (questIndex != -1)
            {
                for (int i = questIndex; i >= 0; i--)
                {
                    var currentQuest = chain.QuestsInOrder[i];
                    if (currentQuest.IsCheckpoint)
                    {
                        return currentQuest;
                    }
                }
                return null;
            }
        }
        return null;
    }
}