using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public sealed class AudioRegistry
{
    public event Action Changed;

    private readonly IAddressablesLoader _addr;
    private readonly Dictionary<string, AudioDescriptor> _map = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AudioClip> _urlCache = new(StringComparer.OrdinalIgnoreCase);

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
        _map.TryGetValue(eventKey ?? string.Empty, out var d);
        return d;
    }

    public async UniTask LoadDefaults(AudioConfigSO config)
    {
        if (config == null || config.Events == null) return;

        foreach (var e in config.Events)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.EventKey)) continue;

            var d = new AudioDescriptor { EventKey = e.EventKey };

            if (e.DefaultClipRef != null && e.DefaultClipRef.RuntimeKeyIsValid())
            {
                try { d.Clip = await e.DefaultClipRef.LoadAssetAsync<AudioClip>().Task; }
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

        Changed?.Invoke();
    }

    public async UniTask Apply(Dictionary<string, object> flat)
    {
        if (flat == null) return;

        var incoming = new Dictionary<string, AudioDescriptor>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in flat)
        {
            string key = kv.Key ?? string.Empty;
            if (!key.StartsWith("Audio.", StringComparison.OrdinalIgnoreCase)) continue;

            int dot1 = key.IndexOf('.');
            int dot2 = key.IndexOf('.', dot1 + 1);

            if (dot1 < 0) continue;

            if (dot2 < 0)
            {
                string eventKey = key[(dot1 + 1)..];
                if (!incoming.TryGetValue(eventKey, out var d0))
                {
                    d0 = new AudioDescriptor { EventKey = eventKey };
                    incoming[eventKey] = d0;
                }
                string val = kv.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    if (val.Contains("://")) d0.Url = val;
                    else d0.Name = val;
                }
                continue;
            }

            string eventName = key.Substring(dot1 + 1, dot2 - dot1 - 1);
            string field = key[(dot2 + 1)..];

            if (!incoming.TryGetValue(eventName, out var d))
            {
                d = new AudioDescriptor { EventKey = eventName };
                incoming[eventName] = d;
            }

            object v = kv.Value;
            string s = v?.ToString() ?? string.Empty;

            switch (field.ToLowerInvariant())
            {
                case "url": d.Url = s; break;
                case "addr": d.Addr = s; break;
                case "name": d.Name = s; break;
                case "ext": d.Ext = s; break;
                case "volume": if (TryToFloat(v, out var vol)) d.Volume = Mathf.Clamp01(vol); break;
                case "pitch": if (TryToFloat(v, out var pit)) d.Pitch = Mathf.Clamp(pit, 0.1f, 3f); break;
                case "loop": if (TryToBool(v, out var loop)) d.Loop = loop; break;
            }
        }

        foreach (var d in incoming.Values)
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
            
            if (!_map.TryGetValue(d.EventKey, out var baseDesc))
                baseDesc = new AudioDescriptor { EventKey = d.EventKey };

            baseDesc.Clip   = clip   ?? baseDesc.Clip;
            baseDesc.Url    = finalUrl ?? baseDesc.Url;
            baseDesc.Addr   = d.Addr ?? baseDesc.Addr;
            baseDesc.Name   = d.Name ?? baseDesc.Name;
            baseDesc.Ext    = d.Ext  ?? baseDesc.Ext;
            baseDesc.Volume = d.Volume ?? baseDesc.Volume ?? 1f;
            baseDesc.Pitch  = d.Pitch ?? baseDesc.Pitch ?? 1f;
            baseDesc.Loop   = d.Loop  ?? baseDesc.Loop  ?? false;

            _map[d.EventKey] = baseDesc;
        }

        Changed?.Invoke();
    }

    private string ResolveUrl(AudioDescriptor d)
    {
        if (!string.IsNullOrEmpty(d.Url)) return d.Url;
        if (string.IsNullOrEmpty(d.Name)) return null;
        string ext = !string.IsNullOrEmpty(d.Ext) ? d.Ext : _defaultExt;
        if (!ext.StartsWith(".")) ext = "." + ext;
        string slug = Slugify(d.Name);
        string baseUrl = _cdnBase?.TrimEnd('/') ?? string.Empty;
        return string.IsNullOrEmpty(baseUrl) ? slug + ext : baseUrl + "/" + slug + ext;
    }

    private async UniTask<AudioClip> GetOrDownloadClip(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (_urlCache.TryGetValue(url, out var cached) && cached != null) return cached;

        var type = GuessAudioTypeByUrl(url);
        using var req = UnityWebRequestMultimedia.GetAudioClip(UrlUtil.WithCacheBuster(url), type);
        req.timeout = 10;
        req.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
        req.SetRequestHeader("Pragma", "no-cache");

        try { await req.SendWebRequest(); }
        catch { return null; }

        if (req.result != UnityWebRequest.Result.Success) return null;

        var clip = DownloadHandlerAudioClip.GetContent(req);
        if (clip != null) _urlCache[url] = clip;
        return clip;
    }

    private static AudioType GuessAudioTypeByUrl(string url)
    {
        string u = url?.ToLowerInvariant() ?? string.Empty;
        if (u.EndsWith(".ogg")) return AudioType.OGGVORBIS;
        if (u.EndsWith(".mp3")) return AudioType.MPEG;
        if (u.EndsWith(".wav")) return AudioType.WAV;
        return AudioType.MPEG;
    }

    private static string Slugify(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var b = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            char lower = char.ToLowerInvariant(c);
            if ((lower >= 'a' && lower <= 'z') || (lower >= '0' && lower <= '9')) { b.Append(lower); continue; }
            if (lower == '-' || lower == '_' || lower == '.') { b.Append(lower); continue; }
            if (char.IsWhiteSpace(lower)) { b.Append('-'); continue; }
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
            if (v is string s)
            {
                s = s.Trim().ToLowerInvariant();
                if (s is "1" or "true" or "yes" or "on") { b = true; return true; }
                if (s is "0" or "false" or "no" or "off") { b = false; return true; }
            }
            b = Convert.ToBoolean(v); return true;
        }
        catch { b = false; return false; }
    }

    public void ReleaseUnusedAudio()
    {
        _urlCache.Clear();
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
