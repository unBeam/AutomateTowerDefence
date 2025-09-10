using UnityEngine;
using Zenject;

public class PauseController : MonoBehaviour
{
    private IPauseService _pause;
    private int _ticket = -1;

    [Inject]
    public void Construct(IPauseService pause)
    {
        _pause = pause;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_ticket < 0)
            {
                _ticket = _pause.Push(PauseMask.Gameplay);
            }
            else
            {
                _pause.Pop(_ticket);
                _ticket = -1;
            }
        }
    }
}