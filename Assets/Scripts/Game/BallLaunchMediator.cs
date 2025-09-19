using Cysharp.Threading.Tasks;
using UnityEngine;

public class BallLaunchMediator : MonoBehaviour
{
    [SerializeField] private BallLauncher _launcher;
    [SerializeField] private FollowCameraBehaviour _camera;

    private BallConfigSO _config;

    private async void Start()
    {
        await ConfigInitGate.WaitReady();

        _config = ConfigHub.Get<BallConfigSO>("cfg.ball");
        if (_config == null)
        {
            Debug.LogError("[BallLaunchMediator] BallConfigSO not found!");
            return;
        }

        if (_launcher != null) 
            _launcher.Initialize(_config);

        if (_camera != null) 
            _camera.Initialize(_launcher, _config);
    }
}