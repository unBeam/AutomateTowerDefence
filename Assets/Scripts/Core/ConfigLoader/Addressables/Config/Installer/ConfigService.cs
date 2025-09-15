using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ConfigService
{
    private readonly IAddressablesLoader _addr;
    private readonly IRemoteTextProvider _remote;

    private readonly Dictionary<string, LiveConfigSO> _loaded = new();

    public ConfigService(IAddressablesLoader addr, IRemoteTextProvider remote)
    {
        _addr = addr;
        _remote = remote;
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

        ConfigInitGate.MarkReady();
    }

    public IReadOnlyDictionary<string, LiveConfigSO> All() => _loaded;

    public T Get<T>(string key) where T : LiveConfigSO
    {
        return _loaded.TryGetValue(key, out LiveConfigSO so) ? so as T : null;
    }
}