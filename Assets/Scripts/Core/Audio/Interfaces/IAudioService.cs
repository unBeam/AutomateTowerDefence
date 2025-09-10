using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAudioService
{
    UniTask Preload(string address);
    UniTask PlaySfx(string address, Vector3 position, float volume);
    UniTask PlayMusic(string address, float volume, bool loop);
    void StopMusic();
    void SetBusVolume(string bus, float volume);
}
