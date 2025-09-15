using UnityEngine;
using Zenject;

public abstract class GameBehaviour : MonoBehaviour
{
    protected VFXManager _vfxManager;

    [Inject]
    protected virtual void Construct(VFXManager vfxManager)
    {
        _vfxManager = vfxManager;
    }
    
    protected virtual void Start() { }
    
    private void Update()
    {
        Tick();
    }
    
    private void FixedUpdate()
    {
        PhysicTick();
    }

    public virtual void Initialize() { }
    protected virtual void Tick() { }
    protected virtual void PhysicTick() { }
}
