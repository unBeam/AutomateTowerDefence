using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ConfigAutoApplier
{
    public static void Apply(
        Dictionary<string, object> flat,     
        IEnumerable<LiveConfigSO> targets,   
        string globalVersionKey = "Version"
    )
    {
        if (flat == null) return;
        
        Dictionary<string, Dictionary<string, object>> sections = SplitBySection(flat);
        Debug.Log($"[Config] sections parsed: {sections.Count}");

        foreach (LiveConfigSO so in targets)
        {
            if (so == null) continue;

            Type t = so.GetType();
            string section = t.GetCustomAttribute<ConfigSectionAttribute>()?.Name ?? t.Name;

            sections.TryGetValue(section, out var data);

            bool changed = false;
            if (data != null)
            {
                Debug.Log($"[Config][{section}] applying {data.Count} keys…");
                changed |= ApplyToObject(so, data);
            }
            else
            {
                Debug.Log($"[Config][{section}] no section data found.");
            }
            
            object verObj = null;
            foreach (var kv in flat)
            {
                if (string.Equals(kv.Key, globalVersionKey, StringComparison.OrdinalIgnoreCase))
                { verObj = kv.Value; break; }
            }
            if (verObj != null)
            {
                changed |= TrySetMember(so, "Version", verObj);
            }

            if (changed)
            {
                t.GetMethod("RaiseChanged", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(so, null);
                Debug.Log($"[Config][{section}] applied & raised Changed.");
            }
        }
    }

    private static Dictionary<string, Dictionary<string, object>> SplitBySection(Dictionary<string, object> flat)
    {
        var map = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in flat)
        {
            string key = kv.Key ?? "";
            int dot = key.IndexOf('.');
            if (dot <= 0) continue;

            string section = key.Substring(0, dot);
            string name = key.Substring(dot + 1);

            if (!map.TryGetValue(section, out var sect))
            {
                sect = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                map[section] = sect;
            }
            sect[name] = kv.Value;
        }
        return map;
    }

    private static bool ApplyToObject(object target, Dictionary<string, object> kvs)
    {
        bool any = false;
        foreach (var kv in kvs)
        {
            if (TrySetMember(target, kv.Key, kv.Value))
            {
                Debug.Log($"[Config][{target.GetType().Name}] set {kv.Key} = {kv.Value}");
                any = true;
            }
            else
            {
                Debug.LogWarning($"[Config][{target.GetType().Name}] key not found: {kv.Key}");
            }
        }
        return any;
    }
    
    private static bool TrySetMember(object target, string logicalName, object value)
    {
        Type t = target.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        FieldInfo f = t.GetField(logicalName, flags);
        if (TrySetField(f, target, value)) return true;
        
        if (!string.IsNullOrEmpty(logicalName))
        {
            string lower = char.ToLowerInvariant(logicalName[0]) + logicalName.Substring(1);
            f = t.GetField("_" + lower, flags);
            if (TrySetField(f, target, value)) return true;
            
            f = t.GetField("m_" + logicalName, flags);
            if (TrySetField(f, target, value)) return true;
        }
        
        PropertyInfo p = t.GetProperty(logicalName, flags | BindingFlags.IgnoreCase);
        if (p != null && p.CanWrite && TryConvert(value, p.PropertyType, out object pc))
        {
            p.SetValue(target, pc);
            return true;
        }

        return false;
    }

    private static bool TrySetField(FieldInfo f, object target, object value)
    {
        if (f == null) return false;
        if (!TryConvert(value, f.FieldType, out object conv)) return false;
        f.SetValue(target, conv);
        return true;
    }

    private static bool TryConvert(object src, Type dstType, out object result)
    {
        try
        {
            if (src == null) { result = null; return !dstType.IsValueType; }

            Type st = src.GetType();
            if (dstType.IsAssignableFrom(st)) { result = src; return true; }

            if (dstType == typeof(string)) { result = src.ToString(); return true; }
            if (dstType == typeof(int))    { result = Convert.ToInt32(src);  return true; }
            if (dstType == typeof(float))  { result = Convert.ToSingle(src); return true; }
            if (dstType == typeof(double)) { result = Convert.ToDouble(src); return true; }
            if (dstType == typeof(bool))
            {
                if (src is string s)
                {
                    s = s.Trim().ToLowerInvariant();
                    if (s is "1" or "true" or "yes" or "on")  { result = true;  return true; }
                    if (s is "0" or "false" or "no"  or "off") { result = false; return true; }
                }
                result = Convert.ToBoolean(src);
                return true;
            }
        }
        catch { /* ignore */ }

        result = null;
        return false;
    }
}
