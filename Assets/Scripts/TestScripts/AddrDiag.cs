// Assets/Scripts/Dev/AddrDiag.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class AddrDiag
{
    public static void DumpStreamingAssetsAA()
    {
        try
        {
            string sa = Application.streamingAssetsPath;
            string aa = Path.Combine(sa, "aa");
            Debug.Log($"[AA] streamingAssets='{sa}' exists={Directory.Exists(sa)}");
            Debug.Log($"[AA] aaDir           ='{aa}' exists={Directory.Exists(aa)}");
            if (Directory.Exists(aa))
            {
                string[] dirs = Directory.GetDirectories(aa, "*", SearchOption.AllDirectories);
                string[] files = Directory.GetFiles(aa, "*", SearchOption.AllDirectories);
                Debug.Log($"[AA] dirs={dirs.Length} files={files.Length}");
                foreach (var d in dirs.Take(6)) Debug.Log("[AA][dir] " + d);
                foreach (var f in files.Take(12)) Debug.Log("[AA][file] " + f);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AA] Dump SA exception: " + e);
        }
    }

    public static void DumpLocatorsAndKeysSample(params object[] sampleKeys)
    {
        try
        {
            var locs = Addressables.ResourceLocators?.ToList();
            Debug.Log($"[AA] locators count = {locs?.Count ?? 0}");
            if (locs != null)
            {
                for (int i = 0; i < locs.Count; i++)
                {
                    var loc = locs[i];
                    Debug.Log($"[AA] locator[{i}]={loc.GetType().Name}");
                    int printed = 0;
                    foreach (var key in loc.Keys)
                    {
                        if (printed++ < 20)
                            Debug.Log($"[AA]  key: {key}");
                        else
                            break;
                    }
                    if (printed == 0) Debug.Log("[AA]  (no keys enumerated)");
                }
            }

            if (sampleKeys != null)
            {
                foreach (var k in sampleKeys)
                {
                    bool exists = Addressables.ResourceLocators.Any(l => l.Keys.Contains(k));
                    Debug.Log($"[AA] hasKey('{k}') = {exists}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AA] Dump locators exception: " + e);
        }
    }
}
