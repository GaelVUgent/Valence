using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WizardEventButton : MonoBehaviour
{
    [SerializeField] protected Button _button;
    [SerializeField] protected TMPro.TextMeshProUGUI _buttonText;

    public string Text { get { return _buttonText.text; } set { _buttonText.text = value; } }
    public Color ButtonColor
    {
        get { return _button.colors.normalColor; }
        set
        {
            ColorBlock block = _button.colors;
            block.normalColor = value;
            block.selectedColor = value;
            _button.colors = block;
        }
    }
    public bool IsInteractable
    {
        get { return _button.interactable;  }
        set { _button.interactable = value; }
    }

    public event System.Action _OnButtonClick;

    protected virtual void Start()
    {
        _button.onClick.AddListener(() => { _OnButtonClick?.Invoke(); });
    }
}
