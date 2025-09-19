using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallLauncher : AbstractGameBehaviour
{
    private Rigidbody _rb;
    private BallConfigSO _config;

    public Vector3 Position => _rb != null ? _rb.position : transform.position;
    public Transform Transform => transform;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.useGravity = true;
    }

    public void Initialize(BallConfigSO config)
    {
        _config = config;
        ApplyConfig();
        if (_config != null) _config.Changed += OnConfigChanged;
    }

    private void ApplyConfig()
    {
        if (_rb == null || _config == null) return;
        _rb.linearDamping = _config.Drag;
    }

    private void OnConfigChanged()
    {
        ApplyConfig();
    }

    public void LaunchWithForce(float force)
    {
        if (_rb == null) return;

        _rb.linearVelocity = Vector3.zero;
        Vector3 dir = (Vector3.forward + Vector3.up * 0.5f).normalized;
        _rb.AddForce(dir * force, ForceMode.VelocityChange);
         _audioHub.Play("Bounds");
         _vfxManager.PlayEffect(VFXKeys.DustEffect, transform);
    }

    public void ResetLaunch()
    {
        if (_rb == null) return;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = Vector3.zero; // если нужно ресетить в 0; можешь убрать
    }

    private void OnCollisionEnter(Collision c)
    {
        if (_config == null) return;
        if (c.contacts.Length == 0) return;

        Vector3 n = c.contacts[0].normal;
        _rb.linearVelocity = Vector3.Reflect(_rb.linearVelocity, n) * _config.Bounce;
        _vfxManager.PlayEffect(VFXKeys.DustEffect, transform);
        _audioHub.Play("Bounds");
    }

    private void OnDisable()
    {
        if (_config != null) _config.Changed -= OnConfigChanged;
    }
}