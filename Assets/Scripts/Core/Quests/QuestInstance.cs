using UniRx;
using System;
using DefaultNamespace.Quests;

public class QuestInstance : IDisposable
{
    public string Id { get; private set; }
    public IReadOnlyReactiveProperty<bool> IsCompleted => _isCompleted;
    public IReadOnlyReactiveProperty<bool> IsFailed => _isFailed;

    private readonly QuestModel _model;
    private readonly IFailCondition _failCondition;
    private readonly ReactiveProperty<bool> _isCompleted = new(false);
    private readonly ReactiveProperty<bool> _isFailed = new(false);
    private readonly CompositeDisposable _disposables = new();

    public QuestInstance(string id, QuestModel model, IFailCondition failCondition = null)
    {
        Id = id;
        _model = model;
        _failCondition = failCondition;
        
        _model.IsCompleted
            .Where(x => x)
            .Subscribe(_ => Complete())
            .AddTo(_disposables);
        
        if (_failCondition != null)
        {
            _failCondition.Initialize();
            _failCondition.IsFailed
                .Where(x => x)
                .Subscribe(_ => Fail())
                .AddTo(_disposables);
        }
    }

    private void Complete()
    {
        if (_isFailed.Value) return;
        _isCompleted.Value = true;
        Dispose();
    }

    private void Fail()
    {
        if (_isCompleted.Value) return;
        _isFailed.Value = true;
        Dispose();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        _model.Dispose();
        _failCondition?.Dispose();
    }
}