using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public sealed class AudioRegistry
{
    public event Action Changed;

    private readonly IAddressablesLoader _addr;
    private readonly Dictionary<string, AudioDescriptor> _map = new Dictionary<string, AudioDescriptor>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AudioClip> _urlCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

    private string _cdnBase = string.Empty;
    private string _defaultExt = ".mp3";

    public AudioRegistry(IAddressablesLoader addressables)
    {
        _addr = addressables;
    }

    public void SetCdnBase(string baseUrl, string defaultExt)
    {
        _cdnBase = baseUrl ?? string.Empty;
        _defaultExt = string.IsNullOrEmpty(defaultExt) ? ".mp3" : (defaultExt.StartsWith(".") ? defaultExt : "." + defaultExt);
    }

    public AudioDescriptor Get(string eventKey)
    {
        AudioDescriptor d;
        _map.TryGetValue(eventKey ?? string.Empty, out d);
        return d;
    }

    public async UniTask LoadDefaults(AudioConfigSO config)
    {
        if (config == null || config.Events == null) return;

        for (int i = 0; i < config.Events.Count; i++)
        {
            AudioConfigSO.AudioEvent e = config.Events[i];
            if (e == null || string.IsNullOrWhiteSpace(e.EventKey)) continue;

            AudioDescriptor d = new AudioDescriptor();
            d.EventKey = e.EventKey;

            if (e.DefaultClipRef != null && e.DefaultClipRef.RuntimeKeyIsValid())
            {
                try
                {
                    d.Clip = await e.DefaultClipRef.LoadAssetAsync<AudioClip>().Task;
                }
                catch { }
            }
            else if (!string.IsNullOrEmpty(e.DefaultAddrOverride))
            {
                d.Clip = await _addr.Load<AudioClip>(e.DefaultAddrOverride);
            }

            d.Volume = e.DefaultVolume;
            d.Pitch = e.DefaultPitch;
            d.Loop = e.DefaultLoop;

            _map[e.EventKey] = d;
        }

        Action handler = Changed;
        if (handler != null) handler();
    }

    public async UniTask Apply(Dictionary<string, object> flat)
    {
        if (flat == null) return;
        
        object basePathObj, defaultExtObj;
        if (flat.TryGetValue("Audio.BasePath", out basePathObj))
        {
            _cdnBase = basePathObj?.ToString() ?? string.Empty;
        }
        if (flat.TryGetValue("Audio.DefaultExt", out defaultExtObj))
        {
            string ext = defaultExtObj?.ToString();
            _defaultExt = string.IsNullOrEmpty(ext) ? ".mp3" : (ext.StartsWith(".") ? ext : "." + ext);
        }

        Dictionary<string, AudioDescriptor> incoming = new Dictionary<string, AudioDescriptor>(StringComparer.OrdinalIgnoreCase);
        
        foreach (KeyValuePair<string, object> kv in flat)
        {
            string key = kv.Key ?? string.Empty;
            if (!key.StartsWith("Audio.", StringComparison.OrdinalIgnoreCase)) continue;

            int dot1 = key.IndexOf('.');
            int dot2 = key.IndexOf('.', dot1 + 1);

            if (dot1 < 0)
                continue;

            if (dot2 < 0)
            {
                string eventKey = key.Substring(dot1 + 1);
                AudioDescriptor d0;
                if (!incoming.TryGetValue(eventKey, out d0))
                {
                    d0 = new AudioDescriptor { EventKey = eventKey };
                    incoming[eventKey] = d0;
                }
                string val = kv.Value != null ? kv.Value.ToString() : string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    if (val.Contains("://")) d0.Url = val;
                    else d0.Name = val;
                }
                continue;
            }

            string eventName = key.Substring(dot1 + 1, dot2 - dot1 - 1);
            string field = key.Substring(dot2 + 1);

            AudioDescriptor d;
            if (!incoming.TryGetValue(eventName, out d))
            {
                d = new AudioDescriptor { EventKey = eventName };
                incoming[eventName] = d;
            }

            object v = kv.Value;
            string s = v != null ? v.ToString() : string.Empty;

            string f = field.ToLowerInvariant();
            if (f == "url") d.Url = s;
            else if (f == "addr") d.Addr = s;
            else if (f == "name") d.Name = s;
            else if (f == "ext") d.Ext = s;
            else if (f == "volume")
            {
                float vol;
                if (TryToFloat(v, out vol)) d.Volume = Mathf.Clamp01(vol);
            }
            else if (f == "pitch")
            {
                float pit;
                if (TryToFloat(v, out pit)) d.Pitch = Mathf.Clamp(pit, 0.1f, 3f);
            }
            else if (f == "loop")
            {
                bool loop;
                if (TryToBool(v, out loop)) d.Loop = loop;
            }
        }

        var loadingTasks = new List<UniTask>();

        foreach (AudioDescriptor d in incoming.Values)
        {
            // Створюємо асинхронну задачу для кожного дескриптора
            // і додаємо її до списку.
            loadingTasks.Add(ProcessSingleDescriptor(d));
        }

        // Чекаємо, поки ВСІ задачі завантаження завершаться
        await UniTask.WhenAll(loadingTasks);
        // --- КІНЕЦЬ ЗМІН ---

        Action changed = Changed;
        if (changed != null) changed();
    }

    private async UniTask ProcessSingleDescriptor(AudioDescriptor d)
    {
        AudioClip clip = null;

        string finalUrl = ResolveUrl(d);

        if (!string.IsNullOrEmpty(finalUrl))
        {
            clip = await GetOrDownloadClip(finalUrl);
            if (clip == null && !string.IsNullOrEmpty(d.Addr))
                clip = await _addr.Load<AudioClip>(d.Addr);
        }
        else if (!string.IsNullOrEmpty(d.Addr))
        {
            clip = await _addr.Load<AudioClip>(d.Addr);
        }

        // Оскільки цей метод буде виконуватися в паралельних потоках,
        // треба захистити доступ до словника _map
        lock (_map)
        {
            AudioDescriptor baseDesc;
            if (!_map.TryGetValue(d.EventKey, out baseDesc))
                baseDesc = new AudioDescriptor { EventKey = d.EventKey };

            baseDesc.Clip = clip ?? baseDesc.Clip;
            baseDesc.Url = finalUrl ?? baseDesc.Url;
            baseDesc.Addr = d.Addr ?? baseDesc.Addr;
            baseDesc.Name = d.Name ?? baseDesc.Name;
            baseDesc.Ext = d.Ext ?? baseDesc.Ext;
            baseDesc.Volume = d.Volume ?? baseDesc.Volume ?? 1f;
            baseDesc.Pitch = d.Pitch ?? baseDesc.Pitch ?? 1f;
            baseDesc.Loop = d.Loop ?? baseDesc.Loop ?? false;

            _map[d.EventKey] = baseDesc;
        }
    }

    private string ResolveUrl(AudioDescriptor d)
    {
        if (!string.IsNullOrEmpty(d.Url)) return d.Url;
        if (string.IsNullOrEmpty(d.Name)) return null;
        string ext = !string.IsNullOrEmpty(d.Ext) ? d.Ext : _defaultExt;
        if (!ext.StartsWith(".")) ext = "." + ext;
        string slug = Slugify(d.Name);
        string baseUrl = _cdnBase != null ? _cdnBase.TrimEnd('/') : string.Empty;
        return string.IsNullOrEmpty(baseUrl) ? slug + ext : baseUrl + "/" + slug + ext;
    }

    private async UniTask<AudioClip> GetOrDownloadClip(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        AudioClip cached;
        if (_urlCache.TryGetValue(url, out cached) && cached != null) return cached;

        AudioType type = GuessAudioTypeByUrl(url);
        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(UrlUtil.WithCacheBuster(url), type))
        {
            req.timeout = 10;
            req.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
            req.SetRequestHeader("Pragma", "no-cache");
        
            Debug.Log($"[AudioRegistry] Downloading clip from URL: {req.url}"); // <-- ДОДАЙТЕ ЦЕЙ ЛОГ

            try { await req.SendWebRequest(); } 
            catch (System.Exception ex) 
            {
                Debug.LogError($"[AudioRegistry] Exception downloading clip: {ex.Message}"); // <-- ДОДАЙТЕ ЦЕЙ ЛОГ
                return null; 
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                // <-- ДОДАЙТЕ ЦЕЙ ЛОГ
                Debug.LogError($"[AudioRegistry] Failed to download clip. Error: {req.error}"); 
                return null;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip != null)
            {
                Debug.Log($"[AudioRegistry] Clip downloaded and created successfully!"); // <-- ДОДАЙТЕ ЦЕЙ ЛОГ
                _urlCache[url] = clip;
            }
            else
            {
                Debug.LogWarning($"[AudioRegistry] Download successful, but GetContent returned null."); // <-- ДОДАЙТЕ ЦЕЙ ЛОГ
            }
            return clip;
        }
    }

    private static AudioType GuessAudioTypeByUrl(string url)
    {
        string u = url != null ? url.ToLowerInvariant() : string.Empty;
        if (u.EndsWith(".ogg")) return AudioType.OGGVORBIS;
        if (u.EndsWith(".mp3")) return AudioType.MPEG;
        if (u.EndsWith(".wav")) return AudioType.WAV;
        return AudioType.MPEG;
    }

    private static string Slugify(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        StringBuilder b = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = char.ToLowerInvariant(s[i]);
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) { b.Append(c); continue; }
            if (c == '-' || c == '_' || c == '.') { b.Append(c); continue; }
            if (char.IsWhiteSpace(c)) { b.Append('-'); continue; }
        }
        return b.ToString();
    }

    private static bool TryToFloat(object v, out float f)
    {
        try { f = Convert.ToSingle(v); return true; } catch { f = 0f; return false; }
    }

    private static bool TryToBool(object v, out bool b)
    {
        try
        {
            if (v is string)
            {
                string s = ((string)v).Trim().ToLowerInvariant();
                if (s == "1" || s == "true" || s == "yes" || s == "on") { b = true; return true; }
                if (s == "0" || s == "false" || s == "no" || s == "off") { b = false; return true; }
            }
            b = Convert.ToBoolean(v); return true;
        }
        catch { b = false; return false; }
    }
}

public sealed class AudioDescriptor
{
    public string EventKey;
    public string Url;
    public string Addr;
    public string Name;
    public string Ext;
    public float? Volume;
    public float? Pitch;
    public bool? Loop;
    public AudioClip Clip;
}
