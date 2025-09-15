using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _label;

    private InventoryItemAsset _item;

    public void Bind(InventoryItemAsset item, System.Action<InventoryItemAsset> onClick)
    {
        _item = item;
        if (_icon != null) _icon.sprite = item.Icon;
        if (_label != null) _label.text = item.DisplayName;
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(_item));
        }
        gameObject.SetActive(true);
    }

    public void Clear()
    {
        _item = null;
        if (_icon != null) _icon.sprite = null;
        if (_label != null) _label.text = "";
        gameObject.SetActive(false);
    }
}