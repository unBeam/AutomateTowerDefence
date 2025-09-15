using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class GameBehaviour : MonoBehaviour
{
    protected bool IsReady { get; private set; }

    public async void Init(GameMediator mediator)
    {
        await ConfigInitGate.WaitReady();
        OnInit(mediator);
        IsReady = true;
    }

    protected abstract void OnInit(GameMediator mediator);
    protected virtual void OnTick() { }
    protected virtual void OnFixedTick() { }

    private void Update()
    {
        if (!IsReady) return;
        OnTick();
    }

    private void FixedUpdate()
    {
        if (!IsReady) return;
        OnFixedTick();
    }
}