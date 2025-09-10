using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MonsterSpawner : MonoBehaviour
{
    private PauseTokenSource _tokenSource;

    [Inject]
    public void Construct(PauseTokenSource tokenSource)
    {
        _tokenSource = tokenSource;
    }

    private void Start()
    {
        SpawnLoop().Forget();
    }

    private async UniTaskVoid SpawnLoop()
    {
        PauseToken token = _tokenSource.GetToken(PauseMask.Gameplay);

        while (true)
        {
            await token.WaitWhilePaused();
            await UniTask.Delay(1000);
            Debug.Log("Spawn monster");
        }
    }
}