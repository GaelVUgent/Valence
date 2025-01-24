using System;
using UnityEngine;
using UnityEngine.UI;

public class BabelMarker : MonoBehaviour
{
    public string key;
    private Text targetText;

    [NonSerialized]
    public int index;

    private void Awake() {
        targetText = GetComponent<Text>();
        /*
        if(string.IsNullOrEmpty(key))
            key = value;
        */
    }

    public string value {
        get {
            return targetText.text;
        }
        set {
            targetText.text = value;
        }
    }

    private void OnEnable() {
        index = BabelTranslator.Register(this);
        if (index < 0)
            Debug.LogWarning("Translator is not available for marker " + name);
    }

    private void OnDisable() {
        BabelTranslator.UnRegister(this, index);
    }
}
