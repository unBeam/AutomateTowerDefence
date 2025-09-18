using System.Collections.Generic;
using UnityEngine;


public class InventoryModel
{
    public int Capacity { get; }

    public List<InventoryItemAsset> Items { get; private set; }

    public InventoryModel(InventoryConfig config)
    {
        Items = config.StartItems;
        Capacity = config.Capacity;
    }

    public InventoryItemAsset GetItem(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return null;
        }

        return Items[index];
    }

    public bool TryAddItem(InventoryItemAsset item)
    {
        if(Items.Count + 1 > Capacity) 
            return false;
        
        Items.Add(item);
        return true;
    }

    public void TryRemoveItem(int index)
    {
        if (Items.Contains(GetItem(index)))
        {
            Items.Remove(GetItem(index));
        }
    }    
    
    public void TryRemoveItem(InventoryItemAsset item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
        }
    }
}
