using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfig", menuName = "Inventory/Config")]
public class InventoryConfig : ScriptableObject
{
    [field: SerializeField]
    [field: Range(1, 100)] 
    public int Capacity { get; private set; } = 16;
    
    [field: SerializeField] public List<InventoryItemAsset> StartItems { get; private set; } = new();
}