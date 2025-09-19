using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _label;

    public void Bind(InventoryItemAsset item, Action onClick)
    {
        if (item == null)
        {
            gameObject.SetActive(true);
            return;
        }

        if (_icon != null) _icon.sprite = item.Icon;
        if (_label != null) _label.text = item.DisplayName;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        if (_icon != null) _icon.sprite = null;
        if (_label != null) _label.text = "";

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
