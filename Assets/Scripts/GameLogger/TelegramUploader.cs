using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public static class TelegramUploader
{
    private const string BotToken = "";
    private const string ChatId = "";

    public static async UniTask SendFileAsync(string filePath, string caption)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddField("chat_id", ChatId);
        form.AddField("caption", caption);
        form.AddBinaryData("document", fileData, Path.GetFileName(filePath), "text/plain");

        using UnityWebRequest www = UnityWebRequest.Post($"https://api.telegram.org/bot{BotToken}/sendDocument", form);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError($"[TelegramUploader] Error: {www.error}");
        else
            Debug.Log("[TelegramUploader] Log uploaded");
    }
}