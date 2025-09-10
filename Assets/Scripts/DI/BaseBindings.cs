using Zenject;

namespace DI
{
    public abstract class BaseBindings : MonoInstaller
    {
        protected void BindNewInstance<T>() => Container
            .BindInterfacesAndSelfTo<T>()
            .AsSingle()
            .NonLazy();

        protected void BindInstance<T>(T instance) =>
            Container
                .BindInterfacesAndSelfTo<T>()
                .FromInstance(instance)
                .AsSingle()
                .NonLazy();
        
        protected void BindNewInstanceTransient<T>() => Container
            .BindInterfacesAndSelfTo<T>()
            .AsTransient()
            .NonLazy();

        protected void BindInstanceTransient<T>(T instance) =>
            Container
                .BindInterfacesAndSelfTo<T>()
                .FromInstance(instance)
                .AsTransient()
                .NonLazy();
    }
}