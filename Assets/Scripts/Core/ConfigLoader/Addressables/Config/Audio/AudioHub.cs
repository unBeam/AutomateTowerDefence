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
        Debug.Log("[AudioHub] Awake.");
    }

    public void Construct(AudioRegistry registry)
    {
        _registry = registry;
        if (_registry == null)
        {
            Debug.LogError("[AudioHub] Construct: registry is NULL!");
            Hud.Show("Audio registry NULL");
            return;
        }
        _registry.Changed += OnChanged;
        Debug.Log("[AudioHub] Construct: OK. Subscribed to Changed.");
        OnChanged();
    }

    private void OnDestroy()
    {
        if (_registry != null) _registry.Changed -= OnChanged;
    }

    private void OnChanged()
    {
        Debug.Log("[AudioHub] OnChanged received.");
    }

    public void Play(string eventKey)
    {
        if (_registry == null)
        {
            Debug.LogError("[AudioHub] Play: registry is NULL. Call Construct() first.");
            Hud.Show("Hub: registry NULL");
            return;
        }

        var d = _registry.Get(eventKey);
        if (d == null)
        {
            Debug.LogWarning("[AudioHub] Play: descriptor NOT FOUND for key='" + eventKey + "'");
            Hud.Show("No desc: " + eventKey);
            return;
        }
        if (d.Clip == null)
        {
            Debug.LogWarning("[AudioHub] Play: descriptor found but Clip is NULL for key='" + eventKey + "'");
            Hud.Show("Clip NULL: " + eventKey);
            return;
        }

        _src.loop   = d.Loop ?? false;
        _src.pitch  = d.Pitch ?? 1f;
        _src.volume = d.Volume ?? 1f;
        _src.clip   = d.Clip;

        Debug.Log($"[AudioHub] Play '{eventKey}' clip='{_src.clip.name}' vol={_src.volume:0.##} pitch={_src.pitch:0.##} loop={_src.loop}");
        _src.Stop();
        _src.Play();
    }

    public void PlayClick() => Play("Click");
}
