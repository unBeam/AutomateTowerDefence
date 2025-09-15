using Zenject;

public class AudioInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AudioRegistry>().AsSingle();
        Container.Bind<AudioHub>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesTo<AudioEntryPoint>().AsSingle().NonLazy();
    }
}

public class AudioEntryPoint : IInitializable
{
    private readonly AudioRegistry _registry;
    private readonly AudioHub _hub;

    public AudioEntryPoint(AudioRegistry registry, AudioHub hub)
    {
        _registry = registry;
        _hub = hub;
    }

    public void Initialize()
    {
        _hub.Init(_registry);
    }
}