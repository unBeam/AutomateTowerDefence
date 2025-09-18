using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ConfigSection("Audio")]
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio")]
public class AudioConfigSO : LiveConfigSO
{
    [SerializeField] private List<AudioEvent> _events = new();

    public IReadOnlyList<AudioEvent> Events => _events;

    [Serializable]
    public class AudioEvent
    {
        public string EventKey = "Click";                  
        public AssetReferenceT<AudioClip> DefaultClipRef;    
        [Range(0f,1f)] public float DefaultVolume = 1f;
        [Range(0.1f,3f)] public float DefaultPitch = 1f;
        public bool DefaultLoop = false;
        public string DefaultAddrOverride;
    }
}