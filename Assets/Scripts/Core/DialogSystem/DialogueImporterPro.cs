#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using Dialogues.Configs;
using Dialogues.Domain;
using UnityEditor;
using UnityEngine;

namespace Dialogues.EditorTools
{
    public class DialogueImporterPro
    {
        private readonly string _urlDialogues;
        private readonly string _urlScriptSettings;
        private readonly string _urlCharacters;
        private readonly string _urlGifts;
        private readonly string _urlRelationships;
        private readonly string _nodesSavePath;
        private readonly string _assetsSavePath;
        private readonly List<DialogCharacter> _charactersAssets;

        public DialogueImporterPro(string urlDialogues, string urlScriptSettings, string urlCharacters, string urlGifts, string urlRelationships, string nodesSavePath, string assetsSavePath, List<DialogCharacter> charactersAssets)
        {
            _urlDialogues = urlDialogues;
            _urlScriptSettings = urlScriptSettings;
            _urlCharacters = urlCharacters;
            _urlGifts = urlGifts;
            _urlRelationships = urlRelationships;
            _nodesSavePath = nodesSavePath;
            _assetsSavePath = assetsSavePath;
            _charactersAssets = charactersAssets;
        }

        public async UniTask RunAsync()
        {
            Debug.Log("[DialogueImporter] Начинаем импорт...");

            List<string> dlg = await LoadCsv(_urlDialogues);
            List<string> settings = await LoadCsv(_urlScriptSettings);
            List<string> chars = await LoadCsv(_urlCharacters);
            List<string> gifts = await LoadCsv(_urlGifts);
            List<string> rel = await LoadCsv(_urlRelationships);

            Debug.Log($"[DialogueImporter] CSV загружены: Dialogues={dlg.Count}, Settings={settings.Count}, Characters={chars.Count}, Gifts={gifts.Count}, Relationships={rel.Count}");

            CharacterCatalogAsset characterCatalog = GenerateCharacterCatalog(chars);
            Debug.Log($"[DialogueImporter] CharacterCatalog обновлён: {characterCatalog.Characters.Count} персонажей");

            ScriptSettingsAsset scriptSettings = GenerateScriptSettings(settings);
            Debug.Log($"[DialogueImporter] ScriptSettings обновлён: {scriptSettings.Entries.Count} скриптов");

            RelationshipProgressionAsset progression = GenerateRelationshipProgression(rel);
            Debug.Log($"[DialogueImporter] RelationshipProgression обновлён: {progression.Levels.Count} уровней");

            GiftCatalogAsset giftCatalog = GenerateGiftCatalog(gifts, _assetsSavePath);
            Debug.Log($"[DialogueImporter] GiftCatalog обновлён: {giftCatalog.Gifts.Count} подарков");

            List<DialogueNodeAsset> nodes = GenerateDialogueNodes(dlg, characterCatalog);
            Debug.Log($"[DialogueImporter] DialogueNodes обновлены: {nodes.Count} узлов");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DialogueImporter] Импорт завершён");
        }


        private static async UniTask<List<string>> LoadCsv(string url)
        {
            Debug.Log("[DialogueImporter] Загружаем CSV: " + url);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string content = await client.GetStringAsync(url);
                    string[] split = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> lines = new List<string>(split.Length);
                    for (int i = 0; i < split.Length; i++)
                        if (!string.IsNullOrWhiteSpace(split[i])) lines.Add(split[i]);

                    Debug.Log($"[DialogueImporter] Загружено {lines.Count} строк из {url}");
                    return lines;
                }
                catch (Exception e)
                {
                    Debug.LogError("[DialogueImporter] Ошибка загрузки CSV: " + url + " | " + e.Message);
                    return new List<string>();
                }
            }
        }


        private CharacterCatalogAsset GenerateCharacterCatalog(List<string> lines)
        {
            if (!Directory.Exists(_assetsSavePath)) Directory.CreateDirectory(_assetsSavePath);
            string path = Path.Combine(_assetsSavePath, "CharacterCatalog.asset");
            CharacterCatalogAsset asset = AssetDatabase.LoadAssetAtPath<CharacterCatalogAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CharacterCatalogAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Characters.Clear();
            if (lines.Count < 2) { EditorUtility.SetDirty(asset); return asset; }

            List<string> header = SplitCsv(lines[0]);
            int idxName = IndexOf(header, "Name", "Character", "Char");
            int idxType = IndexOf(header, "Character Type", "Type");
            int idxRace = IndexOf(header, "Race");
            int idxDesc = IndexOf(header, "Description", "Desc");

            for (int i = 1; i < lines.Count; i++)
            {
                List<string> row = SplitCsv(lines[i]);
                string name = Safe(row, idxName);
                if (string.IsNullOrEmpty(name)) continue;
                DialogueCharacterInfo info = new DialogueCharacterInfo();
                info.Name = name;
                info.CharacterType = Safe(row, idxType);
                info.Race = Safe(row, idxRace);
                info.Description = Safe(row, idxDesc);
                asset.Characters.Add(info);
            }

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private ScriptSettingsAsset GenerateScriptSettings(List<string> lines)
        {
            if (!Directory.Exists(_assetsSavePath)) Directory.CreateDirectory(_assetsSavePath);
            string path = Path.Combine(_assetsSavePath, "ScriptSettings.asset");
            ScriptSettingsAsset asset = AssetDatabase.LoadAssetAtPath<ScriptSettingsAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ScriptSettingsAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Entries.Clear();
            if (lines.Count < 2) { EditorUtility.SetDirty(asset); return asset; }

            List<string> header = SplitCsv(lines[0]);
            int idxChar = IndexOf(header, "Character");
            int idxKey = IndexOf(header, "Script Key", "ScriptKey", "Key");
            int idxRLUnlock = IndexOf(header, "RL Unlock", "RLUnlock");
            int idxRLSuccess = IndexOf(header, "RL Success", "RLSuccess");
            int idxRP = IndexOf(header, "RP");
            int idxDesc = IndexOf(header, "Description", "Desc");

            for (int i = 1; i < lines.Count; i++)
            {
                List<string> row = SplitCsv(lines[i]);
                string key = Safe(row, idxKey);
                if (string.IsNullOrEmpty(key)) continue;
                ScriptSettingEntry e = new ScriptSettingEntry();
                e.CharacterName = Safe(row, idxChar);
                e.ScriptKey = key;
                e.RLUnlock = ToInt(Safe(row, idxRLUnlock));
                e.RLSuccess = ToInt(Safe(row, idxRLSuccess));
                e.RPReward = ToInt(Safe(row, idxRP));
                e.Description = Safe(row, idxDesc);
                asset.Entries.Add(e);
            }

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private RelationshipProgressionAsset GenerateRelationshipProgression(List<string> lines)
        {
            if (!Directory.Exists(_assetsSavePath)) Directory.CreateDirectory(_assetsSavePath);
            string path = Path.Combine(_assetsSavePath, "RelationshipProgression.asset");
            RelationshipProgressionAsset asset = AssetDatabase.LoadAssetAtPath<RelationshipProgressionAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<RelationshipProgressionAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Levels.Clear();
            if (lines.Count < 2) { EditorUtility.SetDirty(asset); return asset; }

            List<string> header = SplitCsv(lines[0]);
            int idxLevel = IndexOf(header, "Level");
            int idxRP = IndexOf(header, "RP");

            for (int i = 1; i < lines.Count; i++)
            {
                List<string> row = SplitCsv(lines[i]);
                string levelStr = Safe(row, idxLevel);
                if (string.IsNullOrEmpty(levelStr)) continue;
                RelationshipLevelEntry e = new RelationshipLevelEntry();
                e.Level = ToInt(levelStr);
                e.RequiredRP = ToInt(Safe(row, idxRP));
                asset.Levels.Add(e);
            }

            asset.Levels.Sort((a, b) => a.Level.CompareTo(b.Level));
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private GiftCatalogAsset GenerateGiftCatalog(List<string> lines, string saveRoot)
        {
            if (!Directory.Exists(_assetsSavePath)) Directory.CreateDirectory(_assetsSavePath);
            string path = Path.Combine(_assetsSavePath, "GiftCatalog.asset");
            GiftCatalogAsset catalog = AssetDatabase.LoadAssetAtPath<GiftCatalogAsset>(path);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GiftCatalogAsset>();
                AssetDatabase.CreateAsset(catalog, path);
            }

            catalog.Gifts.Clear();
            if (lines.Count < 2) { EditorUtility.SetDirty(catalog); return catalog; }

            string giftsFolder = Path.Combine(_assetsSavePath, "Gifts");
            if (!Directory.Exists(giftsFolder)) Directory.CreateDirectory(giftsFolder);

            List<string> header = SplitCsv(lines[0]);
            int idxName = IndexOf(header, "Name", "Gift", "Id");
            int idxCost = IndexOf(header, "Cost", "Price");
            int idxType = IndexOf(header, "Type", "Gift Type");
            int idxRP = IndexOf(header, "RP");
            int idxCharacter = IndexOf(header, "Character");
            int idxCharType = IndexOf(header, "Character Type");
            int idxDesc = IndexOf(header, "Description", "Desc");

            for (int i = 1; i < lines.Count; i++)
            {
                List<string> row = SplitCsv(lines[i]);
                string gid = Safe(row, idxName);
                if (string.IsNullOrEmpty(gid)) continue;

                string assetPath = Path.Combine(giftsFolder, gid + ".asset");
                GiftDefinitionAsset gift = AssetDatabase.LoadAssetAtPath<GiftDefinitionAsset>(assetPath);
                if (gift == null)
                {
                    gift = ScriptableObject.CreateInstance<GiftDefinitionAsset>();
                    AssetDatabase.CreateAsset(gift, assetPath);
                }

                gift.GiftId = gid;
                gift.Cost = ToInt(Safe(row, idxCost));
                gift.RP = ToInt(Safe(row, idxRP));
                gift.CharacterName = Safe(row, idxCharacter);
                gift.CharacterType = Safe(row, idxCharType);
                gift.Description = Safe(row, idxDesc);

                string t = Safe(row, idxType);
                GiftType gt = GiftType.Neutral;
                if (string.Equals(t, "Personal", StringComparison.OrdinalIgnoreCase)) gt = GiftType.Personal;
                else if (string.Equals(t, "By-type", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "ByType", StringComparison.OrdinalIgnoreCase)) gt = GiftType.ByType;
                gift.Type = gt;

                EditorUtility.SetDirty(gift);
                catalog.Gifts.Add(gift);
            }

            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private List<DialogueNodeAsset> GenerateDialogueNodes(List<string> lines, CharacterCatalogAsset charCatalog)
        {
            if (!Directory.Exists(_nodesSavePath)) Directory.CreateDirectory(_nodesSavePath);
            List<DialogueNodeAsset> created = new List<DialogueNodeAsset>();
            if (lines.Count < 2) return created;

            List<string> header = SplitCsv(lines[0]);

            int idxType = IndexOf(header, "Type");
            int idxScriptKey = IndexOf(header, "Script Key", "ScriptKey", "SetName", "Set");
            int idxNodeId = IndexOf(header, "NodeID", "Node Id", "Id");
            int idxCharacter = IndexOf(header, "Character");
            int idxEmotion = IndexOf(header, "EmotionID", "Emotion");
            int idxChoiceParent = IndexOf(header, "Choise_ParentID", "Choice_ParentID", "Choice ParentID");
            int idxChoiceIndex = IndexOf(header, "Choise_index", "Choice_index", "Choice Index");
            int idxNextId = IndexOf(header, "NextID", "Next Id", "Next");
            int idxCallback = IndexOf(header, "Callback", "Action");
            int idxMission = IndexOf(header, "ScriptID", "MissionId", "Mission");

            Dictionary<string, int> langCols = new Dictionary<string, int>();
            for (int i = 0; i < header.Count; i++)
            {
                string h = header[i];
                if (h.StartsWith("Text_", StringComparison.OrdinalIgnoreCase))
                {
                    string lc = h.Substring(5).Trim().ToLowerInvariant();
                    langCols[lc] = i;
                }
            }

            Dictionary<string, DialogueNodeAsset> nodeByKey = new Dictionary<string, DialogueNodeAsset>();
            Dictionary<string, List<(int, DialogueChoiceAsset)>> choices = new Dictionary<string, List<(int, DialogueChoiceAsset)>>();

            for (int r = 1; r < lines.Count; r++)
            {
                List<string> row = SplitCsv(lines[r]);
                string scriptKey = Safe(row, idxScriptKey);
                string nodeId = Safe(row, idxNodeId);
                if (string.IsNullOrEmpty(scriptKey) || string.IsNullOrEmpty(nodeId)) continue;

                string parentId = Safe(row, idxChoiceParent);
                bool isChoice = !string.IsNullOrEmpty(parentId);

                if (isChoice)
                {
                    DialogueChoiceAsset ch = new DialogueChoiceAsset();
                    ch.NodeId = nodeId;
                    ch.NextId = Safe(row, idxNextId);
                    ch.MissionId = Safe(row, idxMission);
                    foreach (KeyValuePair<string, int> kv in langCols)
                    {
                        LocalizedStringEntry le = new LocalizedStringEntry();
                        le.LanguageCode = kv.Key;
                        le.Text = Safe(row, kv.Value).Replace("\\n", "\n");
                        ch.LocalizedTexts.Add(le);
                    }

                    int idx = ToInt(Safe(row, idxChoiceIndex));
                    string parentKey = scriptKey + "#" + parentId;
                    if (!choices.ContainsKey(parentKey)) choices[parentKey] = new List<(int, DialogueChoiceAsset)>();
                    choices[parentKey].Add((idx, ch));
                }
                else
                {
                    string assetName = SafeFile(scriptKey + "_" + nodeId) + ".asset";
                    string path = Path.Combine(_nodesSavePath, assetName);
                    DialogueNodeAsset node = AssetDatabase.LoadAssetAtPath<DialogueNodeAsset>(path);
                    if (node == null)
                    {
                        node = ScriptableObject.CreateInstance<DialogueNodeAsset>();
                        AssetDatabase.CreateAsset(node, path);
                    }

                    node.GetType().GetProperty("ScriptKey").SetValue(node, scriptKey);
                    node.GetType().GetProperty("NodeId").SetValue(node, nodeId);

                    string charName = Safe(row, idxCharacter);
                    DialogCharacter dc = ResolveCharacter(charName);
                    node.GetType().GetProperty("Character").SetValue(node, dc);

                    CharacterEmotion emo = CharacterEmotion.Default;
                    string emoStr = Safe(row, idxEmotion);
                    Enum.TryParse(emoStr, true, out emo);
                    node.GetType().GetProperty("Emotion").SetValue(node, emo);

                    node.GetType().GetProperty("NextId").SetValue(node, Safe(row, idxNextId));
                    node.GetType().GetProperty("MissionId").SetValue(node, Safe(row, idxMission));
                    node.GetType().GetProperty("Callback").SetValue(node, Safe(row, idxCallback));

                    List<LocalizedStringEntry> loc = new List<LocalizedStringEntry>();
                    foreach (KeyValuePair<string, int> kv in langCols)
                    {
                        LocalizedStringEntry le = new LocalizedStringEntry();
                        le.LanguageCode = kv.Key;
                        le.Text = Safe(row, kv.Value).Replace("\\n", "\n");
                        loc.Add(le);
                    }
                    typeof(DialogueNodeAsset).GetField("_localizedTexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(node, loc);

                    EditorUtility.SetDirty(node);

                    string key = scriptKey + "#" + nodeId;
                    nodeByKey[key] = node;
                    created.Add(node);
                }
            }

            foreach (KeyValuePair<string, List<(int, DialogueChoiceAsset)>> kv in choices)
            {
                if (!nodeByKey.TryGetValue(kv.Key, out DialogueNodeAsset parent)) continue;
                List<(int, DialogueChoiceAsset)> list = kv.Value;
                list.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                List<DialogueChoiceAsset> ordered = new List<DialogueChoiceAsset>(list.Count);
                for (int i = 0; i < list.Count; i++) ordered.Add(list[i].Item2);
                parent.GetType().GetProperty("IsChoicePoint").SetValue(parent, true);
                parent.GetType().GetProperty("Choices").SetValue(parent, ordered);
                EditorUtility.SetDirty(parent);
            }

            return created;
        }

        private DialogCharacter ResolveCharacter(string name)
        {
            if (_charactersAssets != null)
            {
                for (int i = 0; i < _charactersAssets.Count; i++)
                    if (_charactersAssets[i] != null && _charactersAssets[i].CharacterName == name) return _charactersAssets[i];
            }
            DialogCharacter[] all = Resources.LoadAll<DialogCharacter>("");
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].CharacterName == name) return all[i];
            return null;
        }

        private static int IndexOf(List<string> header, params string[] names)
        {
            for (int i = 0; i < header.Count; i++)
            {
                string h = header[i].Trim();
                for (int j = 0; j < names.Length; j++)
                    if (string.Equals(h, names[j], StringComparison.OrdinalIgnoreCase)) return i;
            }
            return -1;
        }

        private static string Safe(List<string> row, int idx)
        {
            if (idx < 0) return string.Empty;
            if (idx >= row.Count) return string.Empty;
            return row[idx].Trim();
        }

        private static int ToInt(string s)
        {
            int v;
            if (int.TryParse(s, out v)) return v;
            return 0;
        }

        private static string SafeFile(string s)
        {
            string t = s.Replace("/", "_").Replace("\\", "_").Replace(":", "_").Replace("\"", "_").Replace("<", "_").Replace(">", "_").Replace("?", "_").Replace("*", "_");
            return t;
        }

        private static List<string> SplitCsv(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string current = "";
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { current += '"'; i++; }
                    else inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else current += c;
            }
            result.Add(current);
            return result;
        }
    }

    public static class DialogueImporterMenu
    {
        [MenuItem("Tools/Dialogues/Import All (Pro)")]
        public static async void ImportAll()
        {
            DialogueImportSettings settings = Resources.Load<DialogueImportSettings>("Configs/Dialogue/DialogueImportSettings");
            string baseUrl = "https://docs.google.com/spreadsheets/d/" + settings.SheetId + "/export?format=csv&gid=";

            string urlDialogues = baseUrl + settings.GidDialogues;
            string urlScriptSettings = baseUrl + settings.GidScriptSettings;
            string urlCharacters = baseUrl + settings.GidCharacters;
            string urlGifts = baseUrl + settings.GidGifts;
            string urlRelationships = baseUrl + settings.GidRelationships;

            string nodesPath = "Assets/Resources/Dialogues";
            string assetsPath = "Assets/Resources/Dialogues";

            List<DialogCharacter> chars = new List<DialogCharacter>(Resources.LoadAll<DialogCharacter>(""));

            DialogueImporterPro importer = new DialogueImporterPro(urlDialogues, urlScriptSettings, urlCharacters, urlGifts, urlRelationships, nodesPath, assetsPath, chars);
            await importer.RunAsync();
        }
    }
}
#endif
