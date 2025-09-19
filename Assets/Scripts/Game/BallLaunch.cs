using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallLauncher : AbstractGameBehaviour
{
    private Rigidbody _rb;
    private BallConfigSO _config;
    private bool _launched;

    public Vector3 Position => _rb.position;

    public void Initialize(BallConfigSO config)
    {
        _config = config;
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;

        _config.Changed += OnConfigChanged;
        OnConfigChanged();
    }

    protected override void Tick()
    {
        if (_config == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pressed button");
            Vector3 dir = (Vector3.forward + Vector3.up).normalized;
            _rb.AddForce(dir * _config.LaunchForce, ForceMode.VelocityChange);
            _launched = true;
        }
    }

    private void OnConfigChanged()
    {
        
    }

    private void OnCollisionEnter(Collision c)
    {
        if (_config == null) return;
        if (c.contacts.Length > 0)
        {
            Vector3 n = c.contacts[0].normal;
            _rb.linearVelocity = Vector3.Reflect(_rb.linearVelocity, n) * _config.Bounce;
        }
    }

    private void OnDisable()
    {
        if (_config != null)
            _config.Changed -= OnConfigChanged;
    }
}