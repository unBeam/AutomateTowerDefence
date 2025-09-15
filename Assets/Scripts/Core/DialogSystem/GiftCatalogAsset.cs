using System.Collections.Generic;
using UnityEngine;

namespace Dialogues.Configs
{
    public enum GiftType
    {
        Neutral = 0,
        Personal = 1,
        ByType = 2
    }

    [CreateAssetMenu(fileName = "Gift", menuName = "Dialogues/Gift")]
    public class GiftDefinitionAsset : ScriptableObject
    {
        public string GiftId;
        public int Cost;
        public GiftType Type;
        public int RP;
        public string CharacterName;
        public string CharacterType;
        public string Description;
    }

    [CreateAssetMenu(fileName = "GiftCatalog", menuName = "Dialogues/Gift Catalog")]
    public class GiftCatalogAsset : ScriptableObject
    {
        public List<GiftDefinitionAsset> Gifts = new List<GiftDefinitionAsset>();

        public GiftDefinitionAsset Get(string giftId)
        {
            for (int i = 0; i < Gifts.Count; i++)
                if (Gifts[i].GiftId == giftId) return Gifts[i];
            return null;
        }
    }
}