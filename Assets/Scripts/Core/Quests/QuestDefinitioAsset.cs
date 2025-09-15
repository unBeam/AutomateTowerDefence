using UnityEngine;
using Sirenix.OdinInspector;
using System;
using DefaultNamespace.Quests;

namespace DefaultNamespace.Quests
{
    [CreateAssetMenu(menuName = "Quests/Quest Definition")]
    public class QuestDefinitionAsset : ScriptableObject
    {
        [HorizontalGroup("Row1")]
        [LabelWidth(60)]
        public string Id;
        
        [LabelWidth(80)]
        [LabelText("Чекпойнт")]
        public bool IsCheckpoint;

        [LabelText("Условия выполнения")]
        [InlineProperty]
        public QuestCondition Condition;

        [LabelText("Время до провала (сек)")]
        public bool HasTimeLimit;

        [ShowIf(nameof(HasTimeLimit))]
        [MinValue(1)]
        public float TimeLimitSeconds = 60;

        [LabelText("Награда за выполнение")]
        [SerializeReference]
        [InlineProperty]
        public QuestReward Reward;
        
        [LabelText("Обработчик запуска")]
        [InlineProperty]
        public QuestStartHandler StartHandler;
    }
}