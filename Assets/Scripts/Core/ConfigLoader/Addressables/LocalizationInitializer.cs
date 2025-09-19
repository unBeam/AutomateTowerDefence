using UnityEngine;

public static class LocalizationInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var addressablesLoader = new AddressablesLoader();
        Services.RegisterLoader(addressablesLoader);
    }
}