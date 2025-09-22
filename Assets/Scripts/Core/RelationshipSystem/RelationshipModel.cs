using System;

public class RelationshipModel
{
   public RelationshipConfig Config { get; private set; }
   
    private readonly SaveAndLoadSystem _saveAndLoad;
    private GameData _data;
    
    public event Action<string, RelationshipProgress> OnProgressChanged;

    public RelationshipModel(RelationshipConfig relationshipConfig, SaveAndLoadSystem saveAndLoadSystem)
    {
        Config = relationshipConfig;
        _saveAndLoad = saveAndLoadSystem;
    }

    public void Initialize()
    {
        _data = _saveAndLoad.GameData;
    }

    public void AddPoints(string characterId, int points)
    {
        RelationshipProgress progress = GetOrCreateProgress(characterId);
        int pointsToAdd = points;

        while (pointsToAdd > 0)
        {
            int nextReq = Config.GetPointsForCharacterNextLevel(characterId, 
                progress.RelationshipLevel);

            if (nextReq == int.MaxValue)
            {
                progress.RelationshipPoints += pointsToAdd;
                break;
            }

            int pointsNeeded = nextReq - progress.RelationshipPoints;
            if (pointsToAdd < pointsNeeded)
            {
                progress.RelationshipPoints += pointsToAdd;
                break;
            }

            pointsToAdd -= pointsNeeded;
            progress.RelationshipLevel++;
            progress.RelationshipPoints = 0;
        }
        OnProgressChanged?.Invoke(characterId, progress);
    }

    public void RemovePoints(string characterId, int points)
    {
        RelationshipProgress progress = GetOrCreateProgress(characterId);
        int toRemove = points;

        while (toRemove > 0)
        {
            if (progress.RelationshipPoints >= toRemove)
            {
                progress.RelationshipPoints -= toRemove;
                break;
            }

            toRemove -= progress.RelationshipPoints;
            if (progress.RelationshipLevel == 0)
            {
                progress.RelationshipPoints = 0;
                break;
            }
            
            progress.RelationshipLevel--;
            int prevReq = Config.GetPointsForCharacterNextLevel(characterId,
                    progress.RelationshipLevel);
            
            progress.RelationshipPoints = prevReq == int.MaxValue ? 0 : prevReq - 1;
        }
        OnProgressChanged?.Invoke(characterId, progress);
    }

    public RelationshipProgress GetOrCreateProgress(string id)
    {
        if (!_data.RelationshipsProgress.TryGetValue(id, out RelationshipProgress progress))
        {
            progress = new RelationshipProgress
            {
                RelationshipLevel = 1,
                RelationshipPoints = 0
            };
            _data.RelationshipsProgress[id] = progress;
        }

        return progress;
    }
}