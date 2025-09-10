using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlaySfxOnClick : MonoBehaviour
{
    [SerializeField] private string _eventKey = "Click";
    [SerializeField] private AudioHub _hub;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() => {
            if (_hub != null) _hub.Play(_eventKey);
        });
    }
}