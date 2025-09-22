using DI;
using UnityEngine;

public class RelationshipInstaller : BaseBindings
{
    [SerializeField] private RelationshipView _relationshipView;
    [SerializeField] private RelationshipConfig _relationshipConfig;

    public override void InstallBindings()
    {
        BindInstance(_relationshipConfig);
        BindNewInstance<RelationshipModel>();
        BindInstance(_relationshipView);
        BindNewInstance<RelationshipPresenter>();
    }
}
