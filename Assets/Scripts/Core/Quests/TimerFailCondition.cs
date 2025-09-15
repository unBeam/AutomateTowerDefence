using System;
using DefaultNamespace.Quests;
using UniRx;
using UnityEngine;

public class TimerFailCondition : IFailCondition
{
    private readonly float _durationSeconds;
    private readonly ReactiveProperty<bool> _isFailed = new(false);
    private IDisposable _timerDisposable;

    public IReadOnlyReactiveProperty<bool> IsFailed => _isFailed;

    public TimerFailCondition(float durationSeconds)
    {
        _durationSeconds = durationSeconds;
    }

    public void Initialize()
    {
        _timerDisposable = Observable.Timer(TimeSpan.FromSeconds(_durationSeconds))
            .Subscribe(_ => _isFailed.Value = true);
    }

    public void Dispose()
    {
        _timerDisposable?.Dispose();
    }
}