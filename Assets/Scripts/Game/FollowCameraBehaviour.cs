using UnityEngine;

public class FollowCameraBehaviour : AbstractGameBehaviour
{
    private Transform _camera;
    private BallLauncher _target;
    private BallConfigSO _config;

    private Vector3 _offset = new Vector3(0, 5, -10);
    private float _lerpSpeed = 5f;

    public void Initialize(BallLauncher target, BallConfigSO config)
    {
        _camera = transform;
        _target = target;
        _config = config;

        if (_config != null)
            _config.Changed += OnConfigChanged;
    }

    protected override void Tick()
    {
        if (_target == null) return;
        Vector3 desired = _target.Position + _offset;
        _camera.position = Vector3.Lerp(_camera.position, desired, _lerpSpeed * Time.deltaTime);
    }

    private void OnConfigChanged()
    {
        
    }

    private void OnDisable()
    {
        if (_config != null) _config.Changed -= OnConfigChanged;
    }
}