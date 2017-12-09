using System;
using UnityEngine;
using UnityEngine.UI;

public class UITogglePanel : MonoBehaviour
{
    public Button toggleButton;
    public GameObject togglePanel;

    bool isShowing = false;

    void Start()
    {
        toggleButton.onClick.AddListener(() => isShowing = !isShowing);
    }

    void Update()
    {
        togglePanel.SetActive(isShowing);
    }
}
