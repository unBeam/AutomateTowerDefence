using UnityEngine;

public static class Hud
{
    static string _last;
    public static void Show(string msg)
    {
        _last = msg;
        Debug.Log("[HUD] " + msg);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        var go = new GameObject("HUDOverlay");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<HudOverlay>();
    }

    class HudOverlay : MonoBehaviour
    {
        void OnGUI()
        {
            if (string.IsNullOrEmpty(_last)) return;
            var rect = new Rect(10, 10, Screen.width - 20, 80);
            GUI.color = Color.red;
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(20, 20, rect.width - 40, rect.height - 40), _last);
        }
    }
}