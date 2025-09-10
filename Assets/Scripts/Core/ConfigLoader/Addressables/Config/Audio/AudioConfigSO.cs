using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio")]
public class AudioConfigSO : LiveConfigSO
{
    [SerializeField] private List<Entry> _events = new();

    public IReadOnlyList<Entry> Events => _events;

    [Serializable]
    public class Entry
    {
        public string EventKey = "Click";                    // имя события, например "Click"
        public AssetReferenceT<AudioClip> DefaultClipRef;    // адресабл-клип по умолчанию
        [Range(0f,1f)] public float DefaultVolume = 1f;
        [Range(0.1f,3f)] public float DefaultPitch = 1f;
        public bool DefaultLoop = false;
        public string DefaultAddrOverride; // опционально: явный ключ адресабла, если не используешь AssetReference
    }
}