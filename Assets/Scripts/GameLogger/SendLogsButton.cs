using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class SendLogsButton : MonoBehaviour
{
    [SerializeField] private Button _sendButton;

    private void Awake()
    {
        _sendButton.onClick.AddListener(OnSendClicked);
    }

    private async void OnSendClicked()
    {
        string path = GameLogger.GetLogFilePath();

        if (File.Exists(path))
        {
            GameLogger.Log("Отправка логов пользователем...");
            await TelegramUploader.SendFileAsync(path, "Игровые логи пользователя");
        }
        else
        {
            Debug.LogWarning("[SendLogs] Файл логов не найден");
        }
    }
}