using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class ConfigService
{
    private readonly IAddressablesLoader _addr;
    private readonly IRemoteTextProvider _remote;
    private readonly AudioRegistry _audioRegistry;
    private readonly string _configUrl;

    private readonly Dictionary<string, LiveConfigSO> _loaded = new();

    public ConfigService(
        IAddressablesLoader addr,
        IRemoteTextProvider remote,
        AudioRegistry audioRegistry,
        string configUrl
    )
    {
        _addr = addr;
        _remote = remote;
        _audioRegistry = audioRegistry;
        _configUrl = configUrl;
    }

    public async UniTask Initialize()
    {
        _loaded.Clear();
        ConfigHub.Clear();
        _audioRegistry.ReleaseUnusedAudio();

        await _addr.Initialize();

        var configs = await _addr.LoadAll<LiveConfigSO>("Config");
        foreach (var cfg in configs)
        {
            if (cfg == null) continue;
            _loaded[cfg.name] = cfg;
            ConfigHub.Set(cfg.name, cfg);
        }

        var audioConfig = Get<AudioConfigSO>(nameof(AudioConfigSO));
        if (audioConfig != null)
            await _audioRegistry.LoadDefaults(audioConfig);

        var flatJson = await FetchFlatJson(_configUrl);
        if (flatJson != null)
        {
            ConfigAutoApplier.Apply(flatJson, _loaded.Values, "Version");

            string origin   = UrlUtil.GetOrigin(_configUrl);
            string basePath = Flat.GetString(flatJson, "Audio.BasePath", "/audio/");
            string defExt   = Flat.GetString(flatJson, "Audio.DefaultExt", ".mp3");
            string cdnBase  = UrlUtil.Join(origin, basePath);

            _audioRegistry.SetCdnBase(cdnBase, defExt);
            await _audioRegistry.Apply(flatJson);
        }

        ConfigInitGate.MarkReady();
    }

    private async UniTask<Dictionary<string, object>> FetchFlatJson(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        string txt = await _remote.Fetch(UrlUtil.WithCacheBuster(url));
        if (string.IsNullOrEmpty(txt))
            return null;

        return MiniJson.Deserialize(txt) as Dictionary<string, object>;
    }

    public IReadOnlyDictionary<string, LiveConfigSO> All() => _loaded;

    public T Get<T>(string key) where T : LiveConfigSO
    {
        return _loaded.TryGetValue(key, out LiveConfigSO so) ? so as T : null;
    }

    public void ReleaseAll()
    {
        _addr.Clear(true);
        _audioRegistry.ReleaseUnusedAudio();
        ConfigHub.Clear();
        _loaded.Clear();
        UnityEngine.Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
