using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SingleConfigBootstrap : MonoBehaviour
{
    [Header("Remote")]
    [SerializeField] private string _configUrl = "https://testconfigs-c3b83.web.app/configs/Config.json";

    [Header("UI")]
    [SerializeField] private LoadingScreenStopwatch _loading;

    private IAddressablesLoader _addr;
    private IRemoteTextProvider _remote;

    private async void Awake()
    {
        if (_loading != null) _loading.ShowAndStart();

        _addr   = new AddressablesLoader();
        _remote = new UnityWebRequestTextProvider(10);

        await _addr.Initialize();
        await LoadConfigs();

        var flat = await FetchFlatJson(_configUrl);
        if (flat != null)
            ConfigAutoApplier.Apply(flat, GetTargets(), "Version");

        ConfigInitGate.MarkReady();

        if (_loading != null) _loading.StopAndHide();
    }

    private async UniTask LoadConfigs()
    {
        List<LiveConfigSO> configs = await _addr.LoadAll<LiveConfigSO>("Config");
        foreach (var cfg in configs)
            ConfigHub.Set(cfg);

    }

    private IEnumerable<LiveConfigSO> GetTargets()
    {
        foreach (var kv in ConfigHub.All())
            yield return kv.Value;
    }

    private async UniTask<Dictionary<string, object>> FetchFlatJson(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        string txt = await _remote.Fetch(UrlUtil.WithCacheBuster(url));
        if (string.IsNullOrEmpty(txt)) return null;

        return MiniJson.Deserialize(txt) as Dictionary<string, object>;
    }
}