using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioHub : MonoBehaviour
{
    private AudioSource _src;
    private AudioRegistry _registry;

    private void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
    }

    public void Init(AudioRegistry registry)
    {
        _registry = registry;
    }

    public void Play(string eventKey)
    {
        var d = _registry?.Get(eventKey);
        if (d?.Clip == null) return;

        _src.loop   = d.Loop ?? false;
        _src.pitch  = d.Pitch ?? 1f;
        _src.volume = d.Volume ?? 1f;
        _src.clip   = d.Clip;

        _src.Stop();
        _src.Play();
    }
}