using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelationshipConfig", menuName = "Configs/Relationship Config")]
public class RelationshipConfig: ScriptableObject
{
    [field: SerializeField] public List<CharacterProgressinList> Characters { get; private set; } = new();
    private Dictionary<string, List<RelationshipLevel>> _relationshipsCash = new();
    
    private void OnEnable()
    {
        CreateCash();
    }

    private void CreateCash()
    {
        foreach (CharacterProgressinList character in Characters)
        {
            _relationshipsCash.Add(character.CharacterId, character.PointsPerLevel);
        }
    }
    
    public int GetPointsForCharacterNextLevel(string characterId, int currentLevel)
    {
        if (_relationshipsCash.Count == 0)
        {
            CreateCash();
        }

        int levelIndex = currentLevel - 1;
        
        if (levelIndex < _relationshipsCash[characterId].Count)
        {
            return _relationshipsCash[characterId][levelIndex].RelationshipPoints;
        }
        
        return int.MaxValue;
    }
}