using Cysharp.Threading.Tasks;

public class RelationshipPresenter
{
    private readonly RelationshipModel _model;
    private readonly RelationshipView _view;
    private readonly SaveAndLoadSystem _saveAndLoad;
    private string _currentCharacterId;

    public RelationshipPresenter(RelationshipModel model, RelationshipView view, SaveAndLoadSystem saveAndLoad)
    {
        _model = model;
        _view = view;
        _saveAndLoad = saveAndLoad;

        _model.OnProgressChanged += HandleProgressChanged;
    }

    public void Initialize()
    {
        _model.Initialize();
    }
    
    public void SelectCharacter(string characterId)
    {
        _currentCharacterId = characterId;
        RefreshView();
    }

    private void RefreshView()
    {
        RelationshipProgress prog = _model.GetOrCreateProgress(_currentCharacterId);
        int nextReq = _model.Config
            .GetPointsForCharacterNextLevel(_currentCharacterId, prog.RelationshipLevel);
        
        _view.UpdateView(prog.RelationshipLevel, prog.RelationshipPoints, nextReq);
    }

    public void AddPoints(int points)
    {
        _model.AddPoints(_currentCharacterId, points);
    }

    public void RemovePoints(int points)
    {
        _model.RemovePoints(_currentCharacterId, points);
    }

    private void HandleProgressChanged(string characterId, RelationshipProgress prog)
    {
        if (characterId != _currentCharacterId) return;
        
        int nextReq = _model.Config
            .GetPointsForCharacterNextLevel(characterId, prog.RelationshipLevel);
        _view.UpdateView(prog.RelationshipLevel, prog.RelationshipPoints, nextReq);
    }
}