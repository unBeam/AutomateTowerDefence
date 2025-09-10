using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoadingScreenStopwatch : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private string _prefix = "Загрузка...";
    [SerializeField, Min(0.1f)] private float _updateIntervalSeconds = 1f;

    private CancellationTokenSource _cts;
    private float _startRealtime;
    private int _lastShownSeconds;

    public float ElapsedSeconds { get { return Time.realtimeSinceStartup - _startRealtime; } }

    public void ShowAndStart()
    {
        gameObject.SetActive(true);
        _startRealtime = Time.realtimeSinceStartup;
        _lastShownSeconds = -1;
        RestartLoop();
    }

    public float StopAndHide()
    {
        float elapsed = ElapsedSeconds;
        CancelLoop();
        gameObject.SetActive(false);
        return elapsed;
    }

    private void RestartLoop()
    {
        CancelLoop();
        _cts = new CancellationTokenSource();
        RunLoopAsync(_cts.Token).Forget();
    }

    private void CancelLoop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async UniTask RunLoopAsync(CancellationToken token)
    {
        UpdateLabel();

        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_updateIntervalSeconds),
                                DelayType.UnscaledDeltaTime,
                                PlayerLoopTiming.Update,
                                token);

            if (token.IsCancellationRequested) break;
            UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        int secs = Mathf.FloorToInt(ElapsedSeconds);
        if (secs == _lastShownSeconds) return;
        _lastShownSeconds = secs;

        if (_label != null)
        {
            _label.text = _prefix + "\n" + secs + " сек";
        }
    }

    private void OnDisable()
    {
        CancelLoop();
    }

    private void OnDestroy()
    {
        CancelLoop();
    }
}
