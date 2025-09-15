using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // не обязательно, но ок

public class RuntimeLogHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _logText;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private int _maxLines = 200;
    [SerializeField] private bool _autoScroll = true;

    private readonly System.Text.StringBuilder _sb = new();

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;

        ForceRebuild();
        ScrollToBottom();
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stacktrace, LogType type)
    {
        string prefix = type switch
        {
            LogType.Error => "[ERR] ",
            LogType.Exception => "[EXC] ",
            LogType.Warning => "[WARN] ",
            _ => "[LOG] "
        };

        _sb.AppendLine(prefix + condition);

        if (_logText != null)
        {
            _logText.text = _sb.ToString();

            // важный порядок: сначала обновить канвасы/лейаут,
            // потом выставлять позицию скролла
            ForceRebuild();

            if (_autoScroll)
            {
                ScrollToBottom();
            }
        }
    }

    private void ForceRebuild()
    {
        if (_logText == null) return;
        // перестроить геометрию TMP, чтобы preferredHeight был актуален
        _logText.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();

        var rt = _logText.rectTransform;
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        var content = rt.parent as RectTransform;
        if (content != null)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        var viewport = _scrollRect != null ? _scrollRect.viewport : null;
        if (viewport != null)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
    }

    private void ScrollToBottom()
    {
        if (_scrollRect == null) return;
        // 0 = низ, 1 = верх
        _scrollRect.verticalNormalizedPosition = 0f;
    }
}
