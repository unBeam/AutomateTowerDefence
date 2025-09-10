using UnityEngine;
using TMPro;
using Zenject;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private float _time;

    private void Update()
    {
        _time += Time.unscaledDeltaTime;
        _text.text = Mathf.FloorToInt(_time).ToString();
    }
} 