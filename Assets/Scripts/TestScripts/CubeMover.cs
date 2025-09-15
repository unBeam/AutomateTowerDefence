using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeMover : GameBehaviour
{
    private Rigidbody _rb;
    private PlayerMoveConfigSO _config;
    private AudioHub _audio;
    private float _axis;
    private float _speed;

    protected override void OnInit(GameMediator mediator)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        
        _config = mediator.Resolve<PlayerMoveConfigSO>();
        _audio  = mediator.Resolve<AudioHub>();

        if (_config != null)
        {
            _config.Changed += OnConfigChanged;
            OnConfigChanged();
        }
    }

    private void OnConfigChanged() => _speed = Mathf.Max(0f, _config.MoveSpeed);

    protected override void OnTick()
    {
        bool pressed = Input.touchCount > 0 || Input.GetMouseButton(0);
        if (pressed) _audio?.Play("Run");
        _axis = pressed ? 1f : 0f;
    }

    protected override void OnFixedTick()
    {
        var v = _rb.linearVelocity;
        v.x = _axis * _speed;
        _rb.linearVelocity = v;
    }

    private void OnDisable()
    {
        if (_config != null) _config.Changed -= OnConfigChanged;
    }
}