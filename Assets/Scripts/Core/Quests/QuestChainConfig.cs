using System.Collections.Generic;
using DefaultNamespace.Quests;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest Chain Config")]
public class QuestChainConfig : ScriptableObject
{
    [Tooltip("Квесты в том порядке, в котором они должны выполняться")]
    public List<QuestDefinitionAsset> QuestsInOrder = new();
}