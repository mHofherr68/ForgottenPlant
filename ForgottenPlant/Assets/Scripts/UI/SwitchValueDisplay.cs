using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchValueDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    [Header("Display Text")]
    [SerializeField] private string onText = "An";
    [SerializeField] private string offText = "Aus";

    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (valueText == null)
            valueText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateValue(slider.value);
        slider.onValueChanged.AddListener(UpdateValue);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(UpdateValue);
    }

    private void UpdateValue(float value)
    {
        bool isOn = value > 0.5f;

        if (valueText != null)
        {
            valueText.text = isOn ? onText : offText;
        }
    }

    public bool GetValue()
    {
        return slider.value > 0.5f;
    }
}