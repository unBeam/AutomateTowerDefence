using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

public sealed class AudioRegistry
{
    public event Action Changed;

    private readonly IAddressablesLoader _addr;
    private readonly Dictionary<string, AudioDescriptor> _map = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AudioClip> _urlCache = new(StringComparer.OrdinalIgnoreCase);

    public AudioRegistry(IAddressablesLoader addressables)
    {
        _addr = addressables;
    }

    public AudioDescriptor Get(string eventKey)
    {
        _map.TryGetValue(eventKey ?? "", out var d);
        return d;
    }

    // 1) прогружаем дефолты из адресабельного SO (если есть)
    public async UniTask LoadDefaults(AudioConfigSO config)
    {
        if (config == null || config.Events == null) return;

        foreach (var e in config.Events)
        {
            if (string.IsNullOrWhiteSpace(e.EventKey)) continue;

            var d = new AudioDescriptor { EventKey = e.EventKey };
            // default clip (через AssetReference или явный Addr)
            if (e.DefaultClipRef != null && e.DefaultClipRef.RuntimeKeyIsValid())
            {
                try { d.Clip = await e.DefaultClipRef.LoadAssetAsync<AudioClip>().Task; } catch { }
            }
            else if (!string.IsNullOrEmpty(e.DefaultAddrOverride))
            {
                d.Clip = await _addr.Load<AudioClip>(e.DefaultAddrOverride);
            }

            d.Volume = e.DefaultVolume;
            d.Pitch  = e.DefaultPitch;
            d.Loop   = e.DefaultLoop;

            _map[e.EventKey] = d;
        }
        Changed?.Invoke();
    }

    // 2) накрываем значениями из плоского JSON (Audio.<Event>.Field)
    public async UniTask Apply(Dictionary<string, object> flat)
    {
        if (flat == null) return;

        // собираем временно описатели из JSON
        var incoming = new Dictionary<string, AudioDescriptor>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in flat)
        {
            var key = kv.Key ?? "";
            if (!key.StartsWith("Audio.", StringComparison.OrdinalIgnoreCase)) continue;

            int dot1 = key.IndexOf('.');
            int dot2 = key.IndexOf('.', dot1 + 1);
            if (dot1 < 0 || dot2 < 0) continue;

            string eventKey = key.Substring(dot1 + 1, dot2 - dot1 - 1);
            string field    = key.Substring(dot2 + 1);

            if (!incoming.TryGetValue(eventKey, out var d))
            {
                d = new AudioDescriptor { EventKey = eventKey };
                incoming[eventKey] = d;
            }

            object v = kv.Value;
            switch (field.ToLowerInvariant())
            {
                case "url":    d.Url = v?.ToString(); break;
                case "addr":   d.Addr = v?.ToString(); break;
                case "volume": if (TryToFloat(v, out var vol)) d.Volume = Mathf.Clamp01(vol); break;
                case "pitch":  if (TryToFloat(v, out var pit)) d.Pitch  = Mathf.Clamp(pit, 0.1f, 3f); break;
                case "loop":   if (TryToBool(v, out var loop)) d.Loop   = loop; break;
            }
        }

        // подгружаем клипы и мёржим в основной словарь
        foreach (var d in incoming.Values)
        {
            AudioClip clip = null;

            if (!string.IsNullOrEmpty(d.Url))
            {
                clip = await GetOrDownloadClip(d.Url);
                if (clip == null && !string.IsNullOrEmpty(d.Addr))
                    clip = await _addr.Load<AudioClip>(d.Addr);
            }
            else if (!string.IsNullOrEmpty(d.Addr))
            {
                clip = await _addr.Load<AudioClip>(d.Addr);
            }

            if (!_map.TryGetValue(d.EventKey, out var baseDesc))
                baseDesc = new AudioDescriptor { EventKey = d.EventKey };

            // мержим
            baseDesc.Clip   = clip ?? baseDesc.Clip;
            baseDesc.Url    = d.Url    ?? baseDesc.Url;
            baseDesc.Addr   = d.Addr   ?? baseDesc.Addr;
            baseDesc.Volume = d.Volume ?? baseDesc.Volume ?? 1f;
            baseDesc.Pitch  = d.Pitch  ?? baseDesc.Pitch  ?? 1f;
            baseDesc.Loop   = d.Loop   ?? baseDesc.Loop   ?? false;

            _map[d.EventKey] = baseDesc;
        }

        Changed?.Invoke();
    }

    private async UniTask<AudioClip> GetOrDownloadClip(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (_urlCache.TryGetValue(url, out var cached) && cached != null) return cached;

        var type = GuessAudioTypeByUrl(url);
        using (var req = UnityWebRequestMultimedia.GetAudioClip(AppendCacheBuster(url), type))
        {
            req.timeout = 10;
            req.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
            req.SetRequestHeader("Pragma", "no-cache");
            try { await req.SendWebRequest(); } catch { return null; }
            if (req.result != UnityWebRequest.Result.Success) return null;

            var clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip != null) _urlCache[url] = clip;
            return clip;
        }
    }

    private static string AppendCacheBuster(string url)
    {
        string sep = url.Contains("?") ? "&" : "?";
        return url + sep + "cb=" + Guid.NewGuid().ToString("N");
    }

    private static AudioType GuessAudioTypeByUrl(string url)
    {
        string u = url.ToLowerInvariant();
        if (u.EndsWith(".ogg")) return AudioType.OGGVORBIS;
        if (u.EndsWith(".mp3")) return AudioType.MPEG;
        if (u.EndsWith(".wav")) return AudioType.WAV;
        return AudioType.MPEG;
    }

    private static bool TryToFloat(object v, out float f)
    { try { f = Convert.ToSingle(v); return true; } catch { f = 0f; return false; } }

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
}

public sealed class AudioDescriptor
{
    public string EventKey;
    public string Url;
    public string Addr;
    public float? Volume;
    public float? Pitch;
    public bool?  Loop;
    public AudioClip Clip;
}
