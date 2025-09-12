using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXConfig", menuName = "Configs/VFX/VFX Config")]
public class VFXConfig : ScriptableObject
{
    [SerializeField] private List<VFXEntry> _effects;

    private Dictionary<string, VFXEntry> _effectMap;

    public void Initialize()
    {
        _effectMap = _effects.ToDictionary(e => e.Key, e => e);
    }

    public VFXEntry GetEntry(string key)
    {
        if (_effectMap == null)
        {
            Initialize();
        }

        _effectMap.TryGetValue(key, out VFXEntry entry);
        return entry;
    }
}