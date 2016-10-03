using strange.extensions.mediation.impl;
using System;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class PlayerResourceView : View
    {
        public TextMeshProUGUI currentEnergyText;
        public TextMeshProUGUI maxEnergyText;
        public MeshRenderer fillRenderer;

        internal void init()
        {
            currentEnergyText = transform.FindChild("CurrentEnergyText").GetComponent<TextMeshProUGUI>();
            maxEnergyText = transform.FindChild("MaxEnergyText").GetComponent<TextMeshProUGUI>();

            var energyBar = GameObject.Find("EnergyContainer").gameObject;
            fillRenderer = energyBar.transform.FindChild("EnergyBarFill").GetComponent<MeshRenderer>();
        }

        void Update()
        {
        }

        internal void updateText(int resource, int max)
        {
            if (currentEnergyText != null)
            {
                currentEnergyText.text = string.Format("{0}", resource);
                maxEnergyText.text = string.Format("{0}", max);
                fillRenderer.material.SetInt("_CurrentHp", resource);
                fillRenderer.material.SetInt("_MaxHp", Math.Max(max, resource));

            }
        }
    }
}

