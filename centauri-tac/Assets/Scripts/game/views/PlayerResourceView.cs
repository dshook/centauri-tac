using strange.extensions.mediation.impl;
using System;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class PlayerResourceView : View
    {
        public bool isOpponent;

        TextMeshProUGUI currentEnergyText;
        TextMeshProUGUI maxEnergyText;
        MeshRenderer fillRenderer;
        MeshRenderer fillRendererPreview;

        internal void init()
        {
            currentEnergyText = transform.FindChild("CurrentEnergyText").GetComponent<TextMeshProUGUI>();
            maxEnergyText = transform.FindChild("MaxEnergyText").GetComponent<TextMeshProUGUI>();

            fillRenderer = transform.FindChild("EnergyBarFill").GetComponent<MeshRenderer>();
            fillRendererPreview = transform.FindChild("EnergyBarFillPreview").GetComponent<MeshRenderer>();
        }

        float turnLengthMs = 0f;
        bool animatingEnergy = false;
        float timerAccum = 0f;
        float maxEnergy = 0f;
        void Update()
        {
            if(!animatingEnergy) return;

            timerAccum += Time.deltaTime;
            
            var progress = Math.Min(maxEnergy, maxEnergy * ((timerAccum * 1000f) / turnLengthMs));
            fillRendererPreview.material.SetFloat("_CurrentHp", progress);
        }

        internal void updateText(int resource, int max)
        {
            if (currentEnergyText != null)
            {
                currentEnergyText.text = string.Format("{0}", resource);
                maxEnergyText.text = string.Format("{0}", max);
                fillRenderer.material.SetFloat("_CurrentHp", resource);
                fillRenderer.material.SetFloat("_MaxHp", Math.Max(max, resource));

            }
        }

        internal void updatePreview(float maxEn, float turnLength)
        {
            maxEnergy = maxEn;
            timerAccum = 0f;
            this.turnLengthMs = turnLength;
            animatingEnergy = true;
            fillRendererPreview.material.SetFloat("_MaxHp", maxEnergy);
        }
    }
}

