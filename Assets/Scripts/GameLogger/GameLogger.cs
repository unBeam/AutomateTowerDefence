using System;
using System.IO;
using UnityEngine;

public static class GameLogger
{
    private static string _logFilePath;

    public static void Initialize(string fileName = "GameLog.txt")
    {
        string dir = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        _logFilePath = Path.Combine(dir, fileName);
        File.WriteAllText(_logFilePath, $"[GameLogger] Initialized at {DateTime.Now}\n");
    }

    public static void Log(string message)
    {
        string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
        File.AppendAllText(_logFilePath, logEntry + "\n");
        Debug.Log(logEntry);
    }

    public static string GetLogFilePath() => _logFilePath;
}