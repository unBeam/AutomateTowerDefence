using System;
using UnityEngine;

public abstract class LiveConfigSO : ScriptableObject
{
    public event Action Changed;
    protected void RaiseChanged() => Changed?.Invoke();
}