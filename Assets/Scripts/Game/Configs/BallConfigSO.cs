using UnityEngine;

[ConfigSection("Ball")]
[CreateAssetMenu(fileName = "BallConfig", menuName = "Configs/Ball")]
public class BallConfigSO : LiveConfigSO
{
    [SerializeField, Min(0f)] private float _launchForce = 10f;
    [SerializeField, Min(0f)] private float _drag = 0.5f;
    [SerializeField, Min(0f)] private float _bounce = 1.2f;
    [SerializeField] private string _version = "local";

    public float LaunchForce => _launchForce;
    public float Drag        => _drag;
    public float Bounce      => _bounce;
    public string Version    => _version;
}