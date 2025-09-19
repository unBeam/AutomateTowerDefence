using Dialogues.Configs;
using Dialogues.Runtime;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Dialogues/Inventory Item")]
public class InventoryItemAsset : ScriptableObject
{
    [field: SerializeField] public string ItemId { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public GiftDefinitionAsset GiftDefinition { get; private set; }

    public void Give(IGiftService giftService)
    {
        giftService.Give(GiftDefinition);
    }
}