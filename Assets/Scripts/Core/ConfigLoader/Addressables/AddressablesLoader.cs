using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressablesLoader : IAddressablesLoader
{
    private readonly Dictionary<string, Object> _cache = new();
    private bool _initialized;

    public async UniTask Initialize()
    {
        if (_initialized) return;
        await Addressables.InitializeAsync().Task;
        _initialized = true;
    }

    public async UniTask<T> Load<T>(string address) where T : Object
    {
        if (_cache.TryGetValue(address, out var existing))
            return (T)existing;

        T asset = null;
        try
        {
            asset = await Addressables.LoadAssetAsync<T>(address).Task;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AddressablesLoader] Load<{typeof(T).Name}> {address}: {e}");
        }

        // ⚠️ Конфиги не кэшируем, остальные — да
        if (asset != null && !(asset is LiveConfigSO))
            _cache[address] = asset;

        return asset;
    }

    public async UniTask Preload<T>(string address) where T : Object
    {
        if (_cache.ContainsKey(address)) return;
        await Load<T>(address);
    }

    public bool TryGetCached<T>(string address, out T asset) where T : Object
    {
        if (_cache.TryGetValue(address, out var found))
        {
            asset = (T)found;
            return true;
        }
        asset = null;
        return false;
    }

    public void Release(string address, bool force = false)
    {
        if (_cache.TryGetValue(address, out var found))
        {
            Addressables.Release(found);
            if (force) Resources.UnloadAsset(found);
            _cache.Remove(address);
        }
    }

    public void Clear(bool force = false)
    {
        foreach (var kv in _cache)
        {
            Addressables.Release(kv.Value);
            if (force && kv.Value != null)
                Resources.UnloadAsset(kv.Value);
        }
        _cache.Clear();
    }

    public async UniTask<List<T>> LoadAll<T>(string label) where T : Object
    {
        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        var loaded = await handle.Task;

        List<T> result = new();
        foreach (var asset in loaded)
        {
            if (asset != null)
                result.Add(asset);
        }

        // ⚠️ Не делаем Release(handle), иначе ассеты подвиснут
        return result;
    }
}
