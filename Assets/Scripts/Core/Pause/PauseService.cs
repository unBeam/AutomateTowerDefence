using System;
using System.Collections.Generic;
using UniRx;

public class PauseService : IPauseService
{
    private readonly ReactiveProperty<PauseMask> _activeMask = new ReactiveProperty<PauseMask>(PauseMask.None);
    private readonly Dictionary<int, PauseMask> _ticketsMask = new Dictionary<int, PauseMask>();
    private int _nextId = 1;

    public IReadOnlyReactiveProperty<PauseMask> ActiveMask { get { return _activeMask; } }

    public int Push(PauseMask mask)
    {
        int id = _nextId++;
        _ticketsMask[id] = mask;
        _activeMask.Value = _activeMask.Value | mask;
        return id;
    }

    public void Pop(int ticket)
    {
        if (!_ticketsMask.TryGetValue(ticket, out PauseMask mask)) return;
        _ticketsMask.Remove(ticket);
        PauseMask newMask = PauseMask.None;
        foreach (KeyValuePair<int, PauseMask> kv in _ticketsMask)
        {
            newMask |= kv.Value;
        }
        _activeMask.Value = newMask;
    }

    public IDisposable PauseScope(PauseMask mask)
    {
        int id = Push(mask);
        return new Scope(this, id);
    }

    public bool IsAnyActive(PauseMask mask)
    {
        return (_activeMask.Value & mask) != 0;
    }

    private class Scope : IDisposable
    {
        private readonly PauseService _service;
        private readonly int _ticket;
        private bool _disposed;

        public Scope(PauseService service, int ticket)
        {
            _service = service;
            _ticket = ticket;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _service.Pop(_ticket);
        }
    }
}