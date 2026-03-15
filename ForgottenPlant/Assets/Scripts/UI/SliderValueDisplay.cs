/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    [Header("Value Range")]
    [SerializeField] private float lowerValue = 0.1f;
    [SerializeField] private float upperValue = 5f;
    [SerializeField] private bool bipolar = false;

    [Header("Display")]
    [SerializeField] private int decimals = 1;

    private void Start()
    {
        UpdateValue(slider.value);
        slider.onValueChanged.AddListener(UpdateValue);
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(UpdateValue);
        }
    }

    private void UpdateValue(float normalizedValue)
    {
        float mappedValue = MapSliderValue(normalizedValue);
        valueText.text = FormatValue(mappedValue);
    }

    private float MapSliderValue(float normalizedValue)
    {
        if (bipolar)
        {
            return Mathf.Lerp(-upperValue, upperValue, normalizedValue);
        }

        return Mathf.Lerp(lowerValue, upperValue, normalizedValue);
    }

    private string FormatValue(float value)
    {
        string format = "F" + Mathf.Clamp(decimals, 0, 4);
        return value.ToString(format);
    }

    public float GetMappedValue()
    {
        return MapSliderValue(slider.value);
    }
}*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    [Header("Mode")]
    [SerializeField] private bool bipolar = false;

    private void Awake()
    {
        // Sicherheit: Referenzen automatisch holen falls vergessen
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

    private void UpdateValue(float normalizedValue)
    {
        float internalValue = GetMappedValue();
        valueText.text = FormatValue(internalValue);
    }

    private string FormatValue(float value)
    {
        float percent = value * 100f;
        return Mathf.RoundToInt(percent) + "%";
    }

    public float GetMappedValue()
    {
        if (bipolar)
        {
            return slider.value - 0.5f;   // -0.5 … +0.5
        }

        return slider.value;              // 0 … 1
    }
}
