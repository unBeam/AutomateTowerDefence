using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class BallLaunchController : MonoBehaviour
{
    [SerializeField] private BallLauncher _ballLauncher; 
    [SerializeField] private BallConfigSO _ballConfig;   
    [SerializeField] private Button _level1Button;
    [SerializeField] private Button _level2Button;
    [SerializeField] private Button _level3Button;

    private void Start()
    {
        if (_level1Button != null) _level1Button.onClick.AddListener(OnLevel1);
        if (_level2Button != null) _level2Button.onClick.AddListener(OnLevel2);
        if (_level3Button != null) _level3Button.onClick.AddListener(OnLevel3);
        if (_ballConfig != null) _ballConfig.Changed += OnConfigChanged;
    }

    private void OnDestroy()
    {
        if (_level1Button != null) _level1Button.onClick.RemoveListener(OnLevel1);
        if (_level2Button != null) _level2Button.onClick.RemoveListener(OnLevel2);
        if (_level3Button != null) _level3Button.onClick.RemoveListener(OnLevel3);
        if (_ballConfig != null) _ballConfig.Changed -= OnConfigChanged;
    }

    private void OnConfigChanged()
    {
    }

    private void TryLaunch(int levelIndex)
    {
        if (_ballLauncher == null || _ballConfig == null) return;
        float force = _ballConfig.GetLaunchForce(levelIndex);
        _ballLauncher.LaunchWithForce(force);
    }

    public void OnLevel1() => TryLaunch(0);
    public void OnLevel2() => TryLaunch(1);
    public void OnLevel3() => TryLaunch(2);
}