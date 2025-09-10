using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Framework.Audio
{
    public class AddressablesAudioService : IAudioService
    {
        private readonly Dictionary<string, AudioClip> _cache = new();
        private readonly List<AudioSource> _sfxPool = new();
        private readonly Transform _root;
        private readonly AudioSource _music;

        public AddressablesAudioService()
        {
            _root = new GameObject("[Audio]").transform;
            _music = _root.gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0f;
        }

        public async UniTask Preload(string address)
        {
            if (_cache.ContainsKey(address)) return;
            AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(address).ToUniTask();
            if (clip != null && !_cache.ContainsKey(address)) _cache[address] = clip;
        }

        public async UniTask PlaySfx(string address, Vector3 position, float volume)
        {
            AudioClip clip;
            if (!_cache.TryGetValue(address, out clip))
            {
                clip = await Addressables.LoadAssetAsync<AudioClip>(address).ToUniTask();
                if (clip == null) return;
                _cache[address] = clip;
            }
            AudioSource src = GetSfxSource();
            src.transform.position = position;
            src.clip = clip;
            src.volume = volume;
            src.loop = false;
            src.spatialBlend = 0f;
            src.Play();
        }

        public async UniTask PlayMusic(string address, float volume, bool loop)
        {
            AudioClip clip;
            if (!_cache.TryGetValue(address, out clip))
            {
                clip = await Addressables.LoadAssetAsync<AudioClip>(address).ToUniTask();
                if (clip == null) return;
                _cache[address] = clip;
            }
            _music.clip = clip;
            _music.volume = volume;
            _music.loop = loop;
            _music.Play();
        }

        public void StopMusic()
        {
            _music.Stop();
        }

        public void SetBusVolume(string bus, float volume)
        {
            if (bus == "music") _music.volume = volume;
        }

        private AudioSource GetSfxSource()
        {
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                AudioSource s = _sfxPool[i];
                if (!s.isPlaying) return s;
            }
            AudioSource created = new GameObject("SFX").AddComponent<AudioSource>();
            created.transform.SetParent(_root);
            created.playOnAwake = false;
            _sfxPool.Add(created);
            return created;
        }
    }
}
