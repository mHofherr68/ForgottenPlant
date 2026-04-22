using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchValueDisplay : MonoBehaviour
{
    [Header("References")]
    // Slider used as the on/off switch input.
    [SerializeField] private Slider slider;

    // Text element used to display the current switch state.
    [SerializeField] private TMP_Text valueText;

    [Header("Display Text")]
    // Text shown when the switch is considered on.
    [SerializeField] private string onText = "An";

    // Text shown when the switch is considered off.
    [SerializeField] private string offText = "Aus";

    private void Awake()
    {
        // Auto-assign the slider if it was not set manually.
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        // Auto-assign the text element if it was not set manually.
        if (valueText == null)
            valueText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        // Update the displayed value immediately when the object becomes active.
        UpdateValue(slider.value);

        // Start listening for slider value changes.
        slider.onValueChanged.AddListener(UpdateValue);
    }

    private void OnDisable()
    {
        // Stop listening for slider value changes when the object becomes inactive.
        slider.onValueChanged.RemoveListener(UpdateValue);
    }

    private void UpdateValue(float value)
    {
        // Treat values above 0.5 as "on".
        bool isOn = value > 0.5f;

        // Update the text display with the configured on/off strings.
        if (valueText != null)
        {
            valueText.text = isOn ? onText : offText;
        }
    }

    public bool GetValue()
    {
        // Return the current slider state as a boolean.
        return slider.value > 0.5f;
    }
}