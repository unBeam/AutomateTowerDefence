using System;
using System.Collections.Generic;

public class GameMediator
{
    private readonly Dictionary<Type, object> _services = new();

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public T Resolve<T>() where T : class
    {
        return _services.TryGetValue(typeof(T), out var s) ? (T)s : null;
    }
}