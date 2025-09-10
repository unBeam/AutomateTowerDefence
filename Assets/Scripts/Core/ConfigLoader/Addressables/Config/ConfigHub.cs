using System.Collections.Generic;
using UnityEngine;

public static class ConfigHub
{
    private static readonly Dictionary<string, LiveConfigSO> _map = new();

    public static void Set(string key, LiveConfigSO so)
    {
        if (string.IsNullOrEmpty(key) || so == null) return;
        _map[key] = so;
    }

    public static T Get<T>(string key) where T : LiveConfigSO
    {
        if (_map.TryGetValue(key, out var so)) return so as T;
        return null;
    }

    public static LiveConfigSO Get(string key)
        => _map.TryGetValue(key, out var so) ? so : null;

    public static void Clear() => _map.Clear();
}