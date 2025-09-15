using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class ConfigConsumer<T> : MonoBehaviour where T : LiveConfigSO
{
    protected T Config { get; private set; }

    private async void Awake()
    {
        await ConfigInitGate.WaitReady();
        Config = ConfigHub.Get<T>(typeof(T).Name);
        if (Config != null) OnConfigReady();
    }

    protected abstract void OnConfigReady();
}