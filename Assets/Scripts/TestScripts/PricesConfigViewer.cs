using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PricesConfigViewer : MonoBehaviour
{
    [SerializeField] private string _configKey = "Pricesss";
    [SerializeField] private TMP_Text _text;

    private PricesConfigSO _config;
    private CancellationTokenSource _cts;

    private void OnEnable()
    {
        if (_text == null) _text = GetComponentInChildren<TMP_Text>();
        _cts = new CancellationTokenSource();
        BindAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (_config != null) _config.Changed -= OnConfigChanged;
        _config = null;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTaskVoid BindAsync(CancellationToken ct)
    {
        if (!ConfigInitGate.IsReady)
            await ConfigInitGate.WaitReady().AttachExternalCancellation(ct);

        if (ct.IsCancellationRequested) return;

        _config = ConfigHub.Get<PricesConfigSO>(_configKey);
        Debug.Log($"[PricesConfigViewer] Try get key={_configKey}, total={ConfigHub.All().Count}");
        foreach (var kv in ConfigHub.All())
            Debug.Log($"[ConfigHub] contains key={kv.Key}");
        if (_config == null)
        {
            Debug.LogWarning("[PricesConfigViewer] Config not found after ready.");
            return;
        }

        _config.Changed += OnConfigChanged;
        OnConfigChanged();
    }

    private void OnConfigChanged()
    {
        if (_text == null || _config == null) return;

        _text.text =
            "Version: "   + _config.Version;
    }
}