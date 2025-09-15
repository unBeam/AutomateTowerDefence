using UnityEngine;
using System;

public abstract class LiveConfigSO : ScriptableObject
{
    [SerializeField] private string _key;
    public string Key => _key;

    public event Action Changed;
    protected void RaiseChanged() => Changed?.Invoke();
}