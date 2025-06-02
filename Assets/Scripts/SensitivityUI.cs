using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class SensitivityUI : MonoBehaviour
{
    [Header("UI references")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_InputField displayField;
    [SerializeField] private FirstPersonController look;

    private const string Key = "MouseSensitivity";

    private void Awake()
    {
        float saved = Mathf.Clamp(PlayerPrefs.GetFloat(Key, 1f), slider.minValue, slider.maxValue);

        slider.SetValueWithoutNotify(saved);
        SyncDisplay(saved);

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(OnSlider);

        displayField.onEndEdit.RemoveAllListeners();
        displayField.onEndEdit.AddListener(OnDisplayEdit);
        displayField.onSubmit.RemoveAllListeners();
        displayField.onSubmit.AddListener(OnDisplayEdit);
    }


    private void OnSlider(float v)
    {
        Apply(v, save: true);
    }

    private void OnDisplayEdit(string input)
    {
        if (!float.TryParse(input, out float v))
        {
            SyncDisplay(slider.value);
            return;
        }

        v = Mathf.Clamp(v, slider.minValue, slider.maxValue);

        slider.SetValueWithoutNotify(v);
        Apply(v, save: true);
    }

    private void Apply(float value, bool save)
    {
        look.SetSensitivity(value);
        SyncDisplay(value);
        if (save) PlayerPrefs.SetFloat(Key, value);
    }

    private void SyncDisplay(float value)
    {
        displayField.SetTextWithoutNotify($"{value:0.0000}");
    }
}
