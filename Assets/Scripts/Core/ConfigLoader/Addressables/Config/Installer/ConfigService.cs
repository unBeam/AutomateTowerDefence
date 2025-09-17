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
        await _addr.Initialize();

        List<LiveConfigSO> configs = await _addr.LoadAll<LiveConfigSO>("Config");
        foreach (var cfg in configs)
        {
            if (cfg == null) continue;
            string key = cfg.name;
            _loaded[key] = cfg;
            ConfigHub.Set(key, cfg);
        }
        
        var audioConfig = Get<AudioConfigSO>(nameof(AudioConfigSO));
        if (audioConfig != null)
        {
            await _audioRegistry.LoadDefaults(audioConfig);
        }

        var flatJson = await FetchFlatJson(_configUrl);
        if (flatJson != null)
        {
            var genericTargets = _loaded.Values.Where(t => !(t is AudioConfigSO));
            ConfigAutoApplier.Apply(flatJson, genericTargets, "Version");
            
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
}