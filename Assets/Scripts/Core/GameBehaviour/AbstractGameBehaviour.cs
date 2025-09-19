using UnityEngine;
using Zenject;

public abstract class AbstractGameBehaviour : MonoBehaviour
{
    protected VFXManager _vfxManager;
    protected AudioHub _audioHub;

    [Inject]
    protected virtual void Construct(VFXManager vfxManager, AudioHub audioHub)
    {
        _vfxManager = vfxManager;
        _audioHub = audioHub;
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
