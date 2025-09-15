using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class ButtonClickHandler : MonoBehaviour
{
    [SerializeField] private string _buttonName;
    
    [Inject] private QuestEventBus _eventBus;
    private Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(PublishEvent);
    }

    public void SetButtonName(string buttonName)
    {
        _buttonName = buttonName;
    }

    private void PublishEvent()
    {
        _eventBus.Publish(new ButtonClickEvent(_buttonName));
    }

    private void OnDestroy()
    {
        if(_button != null) _button.onClick.RemoveAllListeners();
    }
}