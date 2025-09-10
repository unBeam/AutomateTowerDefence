using UnityEngine.Networking;

public static class UrlUtil
{
    public static string WithQuery(string url, string key, string value)
    {
        string sep = url.Contains("?") ? "&" : "?";
        return $"{url}{sep}{UnityWebRequest.EscapeURL(key)}={UnityWebRequest.EscapeURL(value)}";
    }

    public static string WithCacheBuster(string url)
        => WithQuery(url, "cb", System.Guid.NewGuid().ToString("N"));
}