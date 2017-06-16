using ctac;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Updates a label with the current value and moves it to above the slider
public class SliderLabeler : MonoBehaviour
{
    public GameObject label;
    public GameObject handle;

    public float sentinalValue;
    public string sentinalText;

    Slider slider;
    TextMeshProUGUI text;

    float originalLabelX;
    float originalHandleX;

    void Start()
    {
        slider = GetComponent<Slider>();
        if (slider == null || label == null || handle == null)
        {
            Debug.LogWarning("Slider labeler not setup properly");
            return;
        }
        text = label.GetComponent<TextMeshProUGUI>();

        originalLabelX = label.transform.position.x;
        originalHandleX = handle.transform.position.x;

        slider.onValueChanged.AddListener(onSliderChange);

        onSliderChange(slider.value);
    }

    void Update()
    {

    }

    void onSliderChange(float value)
    {
        if (value == sentinalValue)
        {
            text.text = sentinalText;
        }
        else
        {
            text.text = value.ToString();
        }

        label.transform.position = label.transform.position.SetX(originalLabelX + (handle.transform.position.x - originalHandleX));
    }
}
