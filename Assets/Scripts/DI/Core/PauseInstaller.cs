using UnityEngine;

namespace DI.Core
{
    public class PauseInstaller : BaseBindings
    {
        [SerializeField] private GlobalPauseManager _pauseManager;
        [SerializeField] private CubeMover _cubeMover;
        public override void InstallBindings()
        {
            BindInstance(_cubeMover);
            BindInstance(_pauseManager);
            BindNewInstance<PauseService>();
            BindNewInstance<PauseTokenSource>();
        }
    }
}