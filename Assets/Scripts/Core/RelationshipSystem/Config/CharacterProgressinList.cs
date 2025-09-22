using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterProgressinList
{
    [field: SerializeField] public string CharacterId { get; private set; }
    [field: SerializeField] public List<RelationshipLevel> PointsPerLevel { get; private set; } = new();

}
