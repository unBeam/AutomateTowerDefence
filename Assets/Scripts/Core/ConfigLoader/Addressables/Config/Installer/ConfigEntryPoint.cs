using Zenject;
using Cysharp.Threading.Tasks;

public class ConfigEntryPoint : IInitializable
{
    private readonly ConfigService _cfg;
    private readonly DiContainer _container;

    public ConfigEntryPoint(ConfigService cfg, DiContainer container)
    {
        _cfg = cfg;
        _container = container;
    }

    public async void Initialize()
    {
        await _cfg.Initialize();
        
        foreach (var kv in _cfg.All())
        {
            var type = kv.Value.GetType();
            _container.Bind(type).FromInstance(kv.Value).AsSingle();
        }
    }
}