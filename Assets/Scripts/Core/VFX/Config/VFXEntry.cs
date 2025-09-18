using System;
using UnityEngine;

[Serializable]
public class VFXEntry
{
    [field: SerializeField] public string Key { get; private set; }
    [field: SerializeField] public GameObject ParticlePrefab { get; private set; }
    [field: SerializeField] public VFXAnimation TweenAnimation { get; private set; }
}