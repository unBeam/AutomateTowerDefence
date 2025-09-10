using Cysharp.Threading.Tasks;
using UniRx;

public class PauseToken
{
    private readonly IPauseService _pause;
    private readonly PauseMask _mask;

    public PauseToken(IPauseService pause, PauseMask mask)
    {
        _pause = pause;
        _mask = mask;
    }

    public async UniTask WaitWhilePaused()
    {
        if (!_pause.IsAnyActive(_mask)) return;
        await _pause.ActiveMask.Where(m => (m & _mask) == 0).First().ToUniTask();
    }

    public bool IsPaused
    {
        get { return _pause.IsAnyActive(_mask); }
    }
}