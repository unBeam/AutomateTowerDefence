using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelationshipView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private Slider _progressSlider;
    
    public void UpdateView(int level, int currentPoints, int neededPoints)
    {
        _levelText.text = $"Level: {level}";
        _pointsText.text = $"{currentPoints} / {neededPoints}";
        
        float fill = neededPoints == 0 ? 1f : currentPoints / (float)neededPoints;
        _progressSlider.value = Mathf.Clamp01(fill);
    }
}