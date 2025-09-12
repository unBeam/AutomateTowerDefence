using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeMover : MonoBehaviour
{
    [SerializeField] private string _configKey = "PlayerMove";

    private PlayerMoveConfigSO _config;
    private Rigidbody _rb;
    private float _axis;
    private float _speed;
    private CancellationTokenSource _cts;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        _cts = new CancellationTokenSource();
        BindConfigAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (_config != null) _config.Changed -= OnConfigChanged;
        _config = null;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTaskVoid BindConfigAsync(CancellationToken ct)
    {
        if (!ConfigInitGate.IsReady)
            await ConfigInitGate.WaitReady().AttachExternalCancellation(ct);

        if (ct.IsCancellationRequested) return;

        _config = ConfigHub.Get<PlayerMoveConfigSO>(_configKey);
        if (_config != null) _config.Changed += OnConfigChanged;

        OnConfigChanged(); // задаст скорость (или дефолт 5f)
    }

    private void OnConfigChanged()
    {
        _speed = _config != null ? Mathf.Max(0f, _config.MoveSpeed) : 5f;
        Debug.Log($"[CubeMover] speed={_speed}");
    }

    private void Update()
    {
        bool pressed = Input.touchCount > 0 || Input.GetMouseButton(0);
        _axis = pressed ? 1f : 0f;
    }

    private void FixedUpdate()
    {
        var v = _rb.linearVelocity;
        v.x = _axis * _speed;
        _rb.linearVelocity = v;
    }
}