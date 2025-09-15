using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private InventorySlotView _slotPrefab;

    private readonly List<InventorySlotView> _slots = new List<InventorySlotView>();

    public void Show(List<InventoryItemAsset> items, System.Action<InventoryItemAsset> onClick)
    {
        Clear();
        for (int i = 0; i < items.Count; i++)
        {
            InventorySlotView slot = GetSlot();
            slot.Bind(items[i], onClick);
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        Clear();
        gameObject.SetActive(false);
    }

    private InventorySlotView GetSlot()
    {
        for (int i = 0; i < _slots.Count; i++)
            if (!_slots[i].gameObject.activeSelf) return _slots[i];
        InventorySlotView slot = Instantiate(_slotPrefab, _gridRoot);
        _slots.Add(slot);
        return slot;
    }

    private void Clear()
    {
        for (int i = 0; i < _slots.Count; i++)
            _slots[i].Clear();
    }
}