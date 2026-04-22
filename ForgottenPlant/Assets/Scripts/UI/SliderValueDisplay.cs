using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueDisplay : MonoBehaviour
{
    [Header("References")]
    // Slider whose value should be displayed as text.
    [SerializeField] private Slider slider;

    // Text element used to show the formatted slider value.
    [SerializeField] private TMP_Text valueText;

    [Header("Mode")]
    // If enabled, the slider value is mapped to a bipolar range (-0.5 to +0.5).
    [SerializeField] private bool bipolar = false;

    private void Awake()
    {
        // Safety fallback:
        // automatically find the slider if no reference was assigned manually.
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        // Safety fallback:
        // automatically find the text element if no reference was assigned manually.
        if (valueText == null)
            valueText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        // Update the displayed value immediately when the object becomes active.
        UpdateValue(slider.value);

        // Start listening to slider value changes.
        slider.onValueChanged.AddListener(UpdateValue);
    }

    private void OnDisable()
    {
        // Stop listening to slider value changes when the object becomes inactive.
        slider.onValueChanged.RemoveListener(UpdateValue);
    }

    private void UpdateValue(float normalizedValue)
    {
        // Convert the slider value into the internal mapped value.
        float internalValue = GetMappedValue();

        // Format the mapped value and display it as text.
        valueText.text = FormatValue(internalValue);
    }

    private string FormatValue(float value)
    {
        // Convert the internal value into a percentage string.
        float percent = value * 100f;
        return Mathf.RoundToInt(percent) + "%";
    }

    public float GetMappedValue()
    {
        // In bipolar mode, map the slider from 0..1 to -0.5..+0.5.
        if (bipolar)
        {
            return slider.value - 0.5f;
        }

        // In normal mode, return the original 0..1 slider value.
        return slider.value;
    }
}