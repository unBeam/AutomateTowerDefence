using System;
using UnityEngine;

[Serializable]
public class RelationshipLevel
{
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public int RelationshipPoints { get; private set; }
}
