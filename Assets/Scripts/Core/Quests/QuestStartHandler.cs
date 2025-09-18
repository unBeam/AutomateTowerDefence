using UnityEngine;
using Zenject;

namespace DefaultNamespace.Quests
{
    public abstract class QuestStartHandler : ScriptableObject
    {
        public virtual void InjectDependencies(DiContainer container)
        {
            container.Inject(this);
        }
        
        public abstract void OnQuestStarted();
    }
}