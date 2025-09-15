using System.ComponentModel;
using UnityEngine;
using Zenject;

namespace Core.ConfigLoader.Addressables.Config
{
    public class ConfigInstaller : MonoInstaller
    {
        [SerializeField] private string _configUrl = "https://testconfigs-c3b83.web.app/configs/Config.json";

        public override void InstallBindings()
        {
            Container.Bind<IAddressablesLoader>().To<AddressablesLoader>().AsSingle();
            Container.Bind<IRemoteTextProvider>().To<UnityWebRequestTextProvider>().AsSingle().WithArguments(10);

            Container.Bind<ConfigService>().AsSingle();
            Container.BindInterfacesTo<ConfigEntryPoint>().AsSingle().NonLazy();
        }
    }
}