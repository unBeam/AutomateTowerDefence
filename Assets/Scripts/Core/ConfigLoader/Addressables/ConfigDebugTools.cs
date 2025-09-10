using System.IO;
using UnityEngine;

public static class ConfigDebugTools
{
    [ContextMenu("ResetGameplayCache")]
    public static void ResetGameplayCache()
    {
        string path = Path.Combine(Application.persistentDataPath, "gameplay_config.json");
        if (File.Exists(path)) File.Delete(path);
        Debug.Log("[Config] gameplay_config.json removed");
    }
}