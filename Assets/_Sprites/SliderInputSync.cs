using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderInputSync : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField inputField;

    public int min = 0;
    public int max = 100;

    void Start()
    {
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;

        slider.onValueChanged.AddListener(OnSliderChanged);
        inputField.onEndEdit.AddListener(OnInputChanged);

        OnSliderChanged(slider.value);
    }

    void OnSliderChanged(float value)
    {
        inputField.text = Mathf.RoundToInt(value).ToString();
    }

    void OnInputChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            result = Mathf.Clamp(result, min, max);
            slider.value = result;
        }
        else
        {
            inputField.text = slider.value.ToString();
        }
    }
}