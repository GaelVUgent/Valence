using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CircleSliderElement : MonoBehaviour
{

    [SerializeField] private Text valueLabel;
    [SerializeField] private Transform sliderCapPivot;
    [SerializeField] private Image[] coloredImages;
    [SerializeField] private Image sliderImage;
    [SerializeField] private Gradient gradient;
    [SerializeField] private bool roundValue;

    public float min = 0f;
    public float max = 1f;

    private float _value = 0f;
    public float value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            TriggerUpdate();
        }
    }

    private void Awake()
    {
        TriggerUpdate();
    }

    private void TriggerUpdate()
    {
        float normValue = Mathf.InverseLerp(min, max, value);
        Color c = gradient.Evaluate(normValue);

        if(roundValue)
            valueLabel.text = ((int)value).ToString();
        else
            valueLabel.text = value.ToString("F2");

        sliderImage.fillAmount = normValue;

        foreach (Image im in coloredImages)
            im.color = c;

        sliderCapPivot.localRotation = Quaternion.Euler(0f, 0f, -360f * normValue);
    }
}
