using System.Collections.Generic;
using System.Linq;
using Dialogues.Configs;
using Dialogues.Domain;
using UnityEngine;

namespace Dialogues.Runtime
{
    public class ResourceDialogueRepository : IDialogueRepository
    {
        private readonly Dictionary<string, List<DialogueNodeAsset>> _byScript = new Dictionary<string, List<DialogueNodeAsset>>();
        private readonly Dictionary<string, DialogueNodeAsset> _byKey = new Dictionary<string, DialogueNodeAsset>();

        public ResourceDialogueRepository()
        {
            DialogueNodeAsset[] all = Resources.LoadAll<DialogueNodeAsset>("Dialogues");
            for (int i = 0; i < all.Length; i++)
            {
                DialogueNodeAsset n = all[i];
                if (n == null) continue;
                if (!_byScript.TryGetValue(n.ScriptKey, out List<DialogueNodeAsset> list))
                {
                    list = new List<DialogueNodeAsset>();
                    _byScript[n.ScriptKey] = list;
                }
                list.Add(n);
                string key = n.ScriptKey + "#" + n.NodeId;
                _byKey[key] = n;
            }
        }

        public DialogueNodeAsset GetNode(DialogueNodeKey key)
        {
            string k = key.ToString();
            if (_byKey.TryGetValue(k, out DialogueNodeAsset n)) return n;
            return null;
        }

        public DialogueNodeAsset GetNode(string scriptKey, string nodeId)
        {
            string k = scriptKey + "#" + nodeId;
            if (_byKey.TryGetValue(k, out DialogueNodeAsset n)) return n;
            return null;
        }

        public DialogueNodeAsset GetStartNode(string scriptKey)
        {
            if (!_byScript.TryGetValue(scriptKey, out List<DialogueNodeAsset> list)) return null;
            HashSet<string> referenced = new HashSet<string>();
            for (int i = 0; i < list.Count; i++)
            {
                DialogueNodeAsset n = list[i];
                if (!string.IsNullOrEmpty(n.NextId)) referenced.Add(scriptKey + "#" + n.NextId);
                for (int c = 0; c < n.Choices.Count; c++)
                    if (!string.IsNullOrEmpty(n.Choices[c].NextId)) referenced.Add(scriptKey + "#" + n.Choices[c].NextId);
            }
            for (int i = 0; i < list.Count; i++)
            {
                DialogueNodeAsset n = list[i];
                string key = scriptKey + "#" + n.NodeId;
                if (!referenced.Contains(key)) return n;
            }
            return list.Count > 0 ? list[0] : null;
        }

        public IEnumerable<DialogueNodeAsset> GetScriptNodes(string scriptKey)
        {
            if (_byScript.TryGetValue(scriptKey, out List<DialogueNodeAsset> list)) return list;
            return Enumerable.Empty<DialogueNodeAsset>();
        }
    }

    public class ScriptSettingsRepository : IScriptSettingsRepository
    {
        private readonly ScriptSettingsAsset _asset;

        public ScriptSettingsRepository(ScriptSettingsAsset asset)
        {
            _asset = asset;
        }

        public ScriptSettingEntry Get(string scriptKey)
        {
            if (_asset == null) return null;
            return _asset.Get(scriptKey);
        }
    }
}
