using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    public event Action<int> OnSlotClicked;

    [SerializeField] private GameObject _panel;
    [SerializeField] private RectTransform _gridRoot;
    [SerializeField] private InventorySlotView _slotPrefab;

    [Header("Layout Settings")]
    [SerializeField] private int _columns = 4;

    private GridLayoutGroup _gridLayout;
    private readonly List<InventorySlotView> _slots = new List<InventorySlotView>();

    private void Awake()
    {
        _gridLayout = _gridRoot.GetComponent<GridLayoutGroup>();
        if (_gridLayout == null)
        {
            Debug.LogError("GridLayoutGroup component not found on the grid root!");
        }
    }

    public void Initialize(int capacity)
    {
        if (_gridLayout != null)
        {
            UpdateGridLayout(capacity);
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            Destroy(_slots[i].gameObject);
        }
        _slots.Clear();

        for (int i = 0; i < capacity; i++)
        {
            InventorySlotView slot = Instantiate(_slotPrefab, _gridRoot);
            int slotIndex = i;
            slot.Bind(null, () => OnSlotClicked?.Invoke(slotIndex));
            _slots.Add(slot);
        }
    }

    private void UpdateGridLayout(int capacity)
    {
        if (_columns <= 0) return;

        float totalWidth = _gridRoot.rect.width;
        float spacing = _gridLayout.spacing.x;
        float padding = _gridLayout.padding.left + _gridLayout.padding.right;

        float cellWidth = (totalWidth - padding - (spacing * (_columns - 1))) / _columns;
        float cellHeight = cellWidth;

        _gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }

    public void Render(IReadOnlyList<InventoryItemAsset> items)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < items.Count)
            {
                int itemIndex = i;
                _slots[i].Bind(items[itemIndex], () => OnSlotClicked?.Invoke(itemIndex));
            }
            else
            {
                _slots[i].Clear();
            }
        }
    }

    public void Show() => _panel.SetActive(true);
    public void Hide() => _panel.SetActive(false);
}