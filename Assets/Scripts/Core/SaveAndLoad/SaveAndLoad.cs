using System;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;

public class SaveAndLoadSystem
{
    private string saveFilePath;

    private const string GAMEDATA_JSON = "gamedata.json";

    private const bool IS_ENC = false;

    private EncryptionUtility _encryptionUtility;

    public GameData GameData { get; private set; }

    public async UniTask InitializeSystem()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, GAMEDATA_JSON);
        _encryptionUtility = new EncryptionUtility();
        await LoadOrCreateGameData();
    }

    private async UniTask LoadOrCreateGameData()
    {
        bool fileExists = await UniTask.Run(() => File.Exists(saveFilePath));

        if (fileExists)
        {
            string fileContent =
                await UniTask.Run(() => File.ReadAllText(saveFilePath));

            if (!string.IsNullOrEmpty(fileContent))
            {
                if (IsJsonEncrypted(fileContent))
                {
                    string json = _encryptionUtility.Decrypt(fileContent);
                    GameData = JsonUtility.FromJson<GameData>(json);
                }
                else
                {
                    GameData = JsonUtility.FromJson<GameData>(fileContent);
                }
            }
            else
            {
                GameData = new GameData();
            }
        }
        else
        {
            GameData = new GameData();
        }

        if (GameData == null)
        {
            GameData = new GameData();
        }

        GameData.ProcessLoadedData();

        await SaveGame();
    }

    public async UniTask SaveGame()
    { ;
        GameData.PrepareDataForSave();
        string json = JsonUtility.ToJson(GameData, true);
        string contentToSave;

        if (IS_ENC)
        {
            contentToSave = _encryptionUtility.Encrypt(json);
        }
        else
        {
            contentToSave = json;
        }
        await UniTask.Run(() => File.WriteAllText(saveFilePath, contentToSave));
    }

    private bool IsJsonEncrypted(string content)
    {
        return content.StartsWith("ENC:");
    }
}