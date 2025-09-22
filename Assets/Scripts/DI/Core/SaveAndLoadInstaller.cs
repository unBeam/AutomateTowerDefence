using DI;

public class SaveAndLoadInstaller : BaseBindings
{
    public override void InstallBindings()
    {
        BindNewInstance<SaveAndLoadSystem>();
    }
}
