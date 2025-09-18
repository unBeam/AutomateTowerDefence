using System;
using System.Collections.Generic;
using Dialogues.Runtime;


public class InventoryPresenter : IDisposable
{
    private readonly InventoryModel _model;
    private readonly InventoryView _view;
    private readonly IGiftService _giftService;
    private readonly InventoryConfig _inventoryConfig;

    public InventoryPresenter(InventoryModel model, InventoryView view, IGiftService giftService, InventoryConfig inventoryConfig)
    {
        _model = model;
        _view = view;
        _giftService = giftService;
        _inventoryConfig = inventoryConfig;
    }

    public void Initialize()
    {
        _view.Initialize(_inventoryConfig.Capacity);
        _view.OnSlotClicked += HandleSlotClicked;
    }

    public void Show()
    {
        _view.Render(_model.Items);
        _view.Show();
    }

    public void Hide()
    {
        _view.Hide();
    }

    private void HandleSlotClicked(int index)
    {
        InventoryItemAsset item = _model.GetItem(index);
        if (item != null)
        {
            item.Give(_giftService);
            _model.TryRemoveItem(item);
            Hide();
        }
    }

    public void Dispose()
    {
        _view.OnSlotClicked -= HandleSlotClicked;
    }
}
