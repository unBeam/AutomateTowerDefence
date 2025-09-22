using System;
using UnityEngine;
using Zenject;

public class RelationshipEntrypoint : MonoBehaviour
{
    private RelationshipPresenter _relationshipPresenter;
    private SaveAndLoadSystem _saveAndLoadSystem;
    
    [Inject]
    private void Construct(RelationshipPresenter relationshipPresenter, SaveAndLoadSystem saveAndLoadSystem)
    {
        _relationshipPresenter = relationshipPresenter;
        _saveAndLoadSystem = saveAndLoadSystem;
    }

    private async void Start()
    {
        await _saveAndLoadSystem.InitializeSystem();
        _relationshipPresenter.Initialize();
        _relationshipPresenter.SelectCharacter("Rose");
    }

    public void AddPoints()
    {
        _relationshipPresenter.AddPoints(300);
    }
    
    public void RemovePoints()
    {
        _relationshipPresenter.RemovePoints(20);
    }

    private async void OnApplicationQuit()
    {
        await _saveAndLoadSystem.SaveGame();
    }
}
