using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace Dialogues.UI
{
    public class DialogViewPro : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _portrait;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Transform _choicesRoot;
        [SerializeField] private Button _choiceButtonPrefab;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _giftButton;
        [SerializeField] private Button _romanceButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Slider _relationshipBar;
        [SerializeField] private TextMeshProUGUI _relationshipLabel;
        [SerializeField] private InventoryView inventoryView;
        private readonly List<Button> _pool = new List<Button>();
        [Inject] private InventoryPresenter _inventoryPresenter;

        public void Show()
        {
            if (_root != null) _root.SetActive(true);
            
            SetInventoryHandler(ShowInventory);
        }
        
        public void ShowInventory()
        {
            HideChoices();
            if (inventoryView != null)
                _inventoryPresenter.Show();
        }

        public void HideInventory()
        {
            if (inventoryView != null)
                _inventoryPresenter.Hide();
        }
        
        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        public void SetPortrait(Sprite sprite)
        {
            if (_portrait != null) _portrait.sprite = sprite;
        }

        public void SetName(string n)
        {
            if (_name != null) _name.text = n;
        }

        public void SetText(string t)
        {
            if (_text != null) _text.text = t;
            if (_scroll != null) _scroll.verticalNormalizedPosition = 0f;
        }

        public void SetRelationship(float progress01, string label)
        {
            if (_relationshipBar != null) _relationshipBar.value = progress01;
            if (_relationshipLabel != null) _relationshipLabel.text = label;
        }

        public void SetNextHandler(Action onClick)
        {
            if (_nextButton == null) return;
            _nextButton.onClick.RemoveAllListeners();
            if (onClick != null) _nextButton.onClick.AddListener(() => onClick());
        }

        public void SetGiftHandler(Action onClick)
        {
            if (_giftButton == null) return;
            _giftButton.onClick.RemoveAllListeners();
            if (onClick != null) _giftButton.onClick.AddListener(() => onClick());
        }

        public void SetRomanceHandler(Action onClick)
        {
            if (_romanceButton == null) return;
            _romanceButton.onClick.RemoveAllListeners();
            if (onClick != null) _romanceButton.onClick.AddListener(() => onClick());
        }

        public void SetInventoryHandler(Action onClick)
        {
            if (_inventoryButton == null) return;
            _inventoryButton.onClick.RemoveAllListeners();
            if (onClick != null) _inventoryButton.onClick.AddListener(() => onClick());
        }

        public void SetExitHandler(Action onClick)
        {
            if (_exitButton == null) return;
            _exitButton.onClick.RemoveAllListeners();
            if (onClick != null) _exitButton.onClick.AddListener(() => onClick());
        }

        public void ShowChoices(List<(string text, Action onClick)> items)
        {
            ClearChoices();
            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                Button b = GetButton();
                TextMeshProUGUI label = b.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = items[index].text;
                b.onClick.AddListener(() => items[index].onClick?.Invoke());
                b.gameObject.SetActive(true);
            }
            if (_nextButton != null) _nextButton.gameObject.SetActive(false);
        }


        public void HideChoices()
        {
            ClearChoices();
            if (_nextButton != null) _nextButton.gameObject.SetActive(true);
        }

        private Button GetButton()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (!_pool[i].gameObject.activeSelf) return _pool[i];
            Button b = Instantiate(_choiceButtonPrefab, _choicesRoot);
            _pool.Add(b);
            return b;
        }

        private void ClearChoices()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                _pool[i].onClick.RemoveAllListeners();
                _pool[i].gameObject.SetActive(false);
            }
        }
    }
}
