using Dialogues.App;
using Dialogues.UI;
using UnityEngine;
using Zenject;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogViewPro _view;
    [SerializeField] private string _scriptKey;

    [Inject] private DialogueService _service;

    private void Start()
    {
        var presenter = _service.CreatePresenter(_view);
        if (presenter.CanStart(_scriptKey, "Rose"))
            presenter.Start(_scriptKey, null);
    }
}