using Zenject;

public class PauseTokenSource
{
    private readonly IPauseService _pause;

    [Inject]
    public PauseTokenSource(IPauseService pause)
    {
        _pause = pause;
    }

    public PauseToken GetToken(PauseMask mask)
    {
        return new PauseToken(_pause, mask);
    }
}