using Dialogues.Configs;

namespace Dialogues.Runtime
{
    public class GiftService : IGiftService
    {
        private readonly GiftCatalogAsset _catalog;
        private readonly IRelationshipService _relationships;

        public GiftService(GiftCatalogAsset catalog, IRelationshipService relationships)
        {
            _catalog = catalog;
            _relationships = relationships;
        }

        public bool CanGive(string giftId, string characterName, string characterType)
        {
            GiftDefinitionAsset gift = _catalog != null ? _catalog.Get(giftId) : null;
            if (gift == null) return false;
            if (gift.Type == GiftType.Personal && gift.CharacterName != characterName) return false;
            if (gift.Type == GiftType.ByType && gift.CharacterType != characterType) return false;
            return true;
        }

        public bool Give(GiftDefinitionAsset gift)
        {
            //GiftDefinitionAsset gift = _catalog != null ? _catalog.Get(giftId) : null;
            if (gift == null) return false;
            string characterName = gift.CharacterName;
            if (!CanGive(gift.GiftId, characterName, gift.CharacterType)) return false;
            _relationships.AddRP(characterName, gift.RP);
            return true;
        }
    }
}