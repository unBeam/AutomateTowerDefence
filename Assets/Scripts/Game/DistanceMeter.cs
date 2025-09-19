using TMPro;
using UnityEngine;

public class DistanceMeter : AbstractGameBehaviour
{
    [SerializeField] private TextMeshProUGUI _distanceMeterText;
    [SerializeField] private BallLauncher _ballTransform;
    private Vector3 _startPosition;

    protected override void Start()
    {
        _startPosition = _ballTransform.transform.position;
    }

    protected override void Tick()
    {
        if (_ballTransform == null) return;
        float distance = Vector3.Distance(_startPosition, _ballTransform.transform.position);
        _distanceMeterText.text = LocalizationManager.Get("UIGame/Distance", distance.ToString("F2"));
    }
}
