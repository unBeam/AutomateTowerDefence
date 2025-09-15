using Zenject;

public class TestBehaviour : GameBehaviour
{
    private TestDILog _testDILog;
    
    [Inject]
    private void Construct(TestDILog testDILog)
    {
        _testDILog = testDILog;
    }

    protected override void Start()
    {
        
    }

    protected override void Tick()
    {
        _vfxManager.PlayEffect(VFXKeys.DustEffect, transform);
        _testDILog.Log();
    }

    protected override void PhysicTick()
    {
        _vfxManager.PlayEffect(VFXKeys.Shake, transform);
    }
}
