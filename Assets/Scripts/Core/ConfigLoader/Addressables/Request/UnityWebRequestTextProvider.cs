using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestTextProvider : IRemoteTextProvider
{
    private readonly int _timeoutSec;

    public UnityWebRequestTextProvider(int timeoutSeconds = 8)
    {
        _timeoutSec = timeoutSeconds;
    }

    public async UniTask<string> Fetch(string url)
    {
        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = _timeoutSec;
            req.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
            req.SetRequestHeader("Pragma", "no-cache");
            req.SetRequestHeader("If-Modified-Since", "Mon, 26 Jul 1997 05:00:00 GMT");

            Debug.Log("[NET] GET " + url);
            try
            {
                await req.SendWebRequest();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[NET] EXCEPTION: " + ex.Message);
                Hud.Show("NET EXC: " + ex.Message);
                return null;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NET] FAIL ({req.responseCode}) {req.error} url={url}");
                if (!string.IsNullOrEmpty(req.downloadHandler?.text))
                    Debug.Log("[NET] BODY: " + req.downloadHandler.text);
                Hud.Show($"NET FAIL {req.responseCode}: {req.error}");
                return null;
            }

            var txt = req.downloadHandler.text;
            Debug.Log("[NET] OK (" + (txt?.Length ?? 0) + " bytes)");
            return txt;
        }
    }
}