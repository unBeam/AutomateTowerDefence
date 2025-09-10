using System;
using UniRx;

public interface IPauseService
{
    IReadOnlyReactiveProperty<PauseMask> ActiveMask { get; }
    int Push(PauseMask mask);
    void Pop(int ticket);
    IDisposable PauseScope(PauseMask mask);
    bool IsAnyActive(PauseMask mask);
}