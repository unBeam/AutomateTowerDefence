using Cysharp.Threading.Tasks;
using UnityEngine;

public class ConfigConsumersEntryPoint : MonoBehaviour
{
    private async void Start()
    {
        await ConfigInitGate.WaitReady();

        var consumers = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var c in consumers)
        {
            var t = c.GetType();
            if (!t.IsSubclassOfRawGeneric(typeof(ConfigConsumer<>))) continue;

            var configType = t.BaseType.GetGenericArguments()[0];
            var config = ConfigHub.Get(configType.Name);

            if (config != null)
            {
                var method = t.GetMethod("Init");
                method?.Invoke(c, new object[] { config });
            }
        }
    }
}

public static class TypeExtensions
{
    public static bool IsSubclassOfRawGeneric(this System.Type toCheck, System.Type generic)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur) return true;
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}