using UnityEngine;
using Zenject;

public class GameplayEntryPoint : MonoBehaviour
{
    [SerializeField] private AudioHub _audioHub;
    
    private GameMediator _mediator;
    //private InventoryPresenter _inventoryPresenter;

    // [Inject]
    // private void Construct(InventoryPresenter inventoryPresenter)
    // {
    //     _inventoryPresenter = inventoryPresenter;
    // }
    
    private async void Start()
    {
        _mediator = new GameMediator();
        
        await ConfigInitGate.WaitReady();
        
        _mediator.Register(ConfigHub.Get<PlayerMoveConfigSO>("cfg.player_move"));
        _mediator.Register(_audioHub);
        
        var behaviours = FindObjectsOfType<GameBehaviour>(true);
        foreach (var b in behaviours)
            b.Init(_mediator);
        
        //_inventoryPresenter.Initialize();
    }
}