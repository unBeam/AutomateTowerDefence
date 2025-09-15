using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace DefaultNamespace.Quests
{
    [Serializable]
    public abstract class QuestReward
    {
        public abstract void Give(IQuestStarter questStarter);
    }

    [Serializable]
    public class CoinsReward : QuestReward
    {
        [LabelText("Количество монет")]
        public int Amount = 100;

        public override void Give(IQuestStarter questStarter)
        {
            Debug.Log($"Выдано {Amount} монет.");
        }
    }
}