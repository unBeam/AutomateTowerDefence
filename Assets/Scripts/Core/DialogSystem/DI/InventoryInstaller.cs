using DI;
using UnityEngine;

public class InventoryInstaller : BaseBindings
{
    [SerializeField] private InventoryView _inventoryView;
    [SerializeField] private InventoryConfig _inventoryConfig;
    public override void InstallBindings()
    {
        BindNewInstance<InventoryModel>();
        BindNewInstance<InventoryPresenter>();
        BindInstance(_inventoryView);
        BindInstance(_inventoryConfig);
    }
}
