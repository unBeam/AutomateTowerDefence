using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogues.Configs
{
    [Serializable]
    public class DialogueCharacterInfo
    {
        public string Name;
        public string CharacterType;
        public string Race;
        [TextArea(3, 10)] public string Description;
    }


    [CreateAssetMenu(fileName = "CharacterCatalog", menuName = "Dialogues/Character Catalog")]
    public class CharacterCatalogAsset : ScriptableObject
    {
        public List<DialogueCharacterInfo> Characters = new List<DialogueCharacterInfo>();

        public DialogueCharacterInfo Get(string name)
        {
            for (int i = 0; i < Characters.Count; i++)
                if (Characters[i].Name == name) return Characters[i];
            return null;
        }
    }

}