using System;
using UniRx;

namespace DefaultNamespace.Quests
{
    public interface IFailCondition : IDisposable
    {
        IReadOnlyReactiveProperty<bool> IsFailed { get; }
        void Initialize();
    }
}