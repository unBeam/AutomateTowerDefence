using UnityEngine;

[ConfigSection("Ball")]
[CreateAssetMenu(fileName = "BallConfig", menuName = "Configs/Ball")]
public class BallConfigSO : LiveConfigSO
{
    [Header("Launch forces for 3 levels")]
    [SerializeField, Min(0f)] private float _launchForceLevel1 = 8f;
    [SerializeField, Min(0f)] private float _launchForceLevel2 = 12f;
    [SerializeField, Min(0f)] private float _launchForceLevel3 = 18f;

    [Header("Physics")]
    [SerializeField, Min(0f)] private float _drag = 0.1f;
    [SerializeField, Min(0f)] private float _bounce = 0.9f;

    [Header("Meta")]
    [SerializeField] private string _version = "local";

    public float LaunchForceLevel1 => _launchForceLevel1;
    public float LaunchForceLevel2 => _launchForceLevel2;
    public float LaunchForceLevel3 => _launchForceLevel3;

    public float Drag   => _drag;
    public float Bounce => _bounce;
    public string Version => _version;

    public float GetLaunchForce(int levelIndex)
    {
        switch (levelIndex)
        {
            case 0: return _launchForceLevel1;
            case 1: return _launchForceLevel2;
            case 2: return _launchForceLevel3;
            default: return _launchForceLevel3;
        }
    }
}