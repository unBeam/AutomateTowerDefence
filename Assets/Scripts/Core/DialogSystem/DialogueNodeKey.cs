using System;

namespace Dialogues.Domain
{
    [Serializable]
    public struct DialogueNodeKey : IEquatable<DialogueNodeKey>
    {
        public string ScriptKey;
        public string NodeId;

        public DialogueNodeKey(string scriptKey, string nodeId)
        {
            ScriptKey = scriptKey;
            NodeId = nodeId;
        }

        public override string ToString()
        {
            return ScriptKey + "#" + NodeId;
        }

        public bool Equals(DialogueNodeKey other)
        {
            return string.Equals(ScriptKey, other.ScriptKey) && string.Equals(NodeId, other.NodeId);
        }

        public override bool Equals(object obj)
        {
            if (obj is DialogueNodeKey key) return Equals(key);
            return false;
        }

        public override int GetHashCode()
        {
            int h1 = ScriptKey != null ? ScriptKey.GetHashCode() : 0;
            int h2 = NodeId != null ? NodeId.GetHashCode() : 0;
            return h1 ^ (h2 << 1);
        }
    }
}