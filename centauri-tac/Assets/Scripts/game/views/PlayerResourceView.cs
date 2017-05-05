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

        ISoundService sounds;

        internal void init(ISoundService s)
        {
            currentEnergyText = transform.FindChild("CurrentEnergyText").GetComponent<TextMeshProUGUI>();
            maxEnergyText = transform.FindChild("MaxEnergyText").GetComponent<TextMeshProUGUI>();

            fillRenderer = transform.FindChild("EnergyBarFill").GetComponent<MeshRenderer>();
            fillRendererPreview = transform.FindChild("EnergyBarFillPreview").GetComponent<MeshRenderer>();
            sounds = s;
        }

        float turnLengthMs = 0f;
        float timerAccum = 0f;
        bool animatingEnergy = false;
        float maxEnergy = 0f;

        bool playedIncomingTurn = false;
        float incomingTurnSoundLength = 2f;
        void Update()
        {
            if(!animatingEnergy) return;

            timerAccum += Time.deltaTime;

            if (!playedIncomingTurn && timerAccum > (turnLengthMs / 1000f) - incomingTurnSoundLength)
            {
                sounds.PlaySound("turnFinish");
                playedIncomingTurn = true;
            }
            
            var progress = Math.Min(maxEnergy, maxEnergy * ((timerAccum * 1000f) / turnLengthMs));
            fillRendererPreview.material.SetFloat("_CurrentHp", progress);
        }

        int prevResource = 0;
        internal void setEnergy(int resource, int max)
        {
            if (currentEnergyText == null) { return; }

            currentEnergyText.text = string.Format("{0}", resource);
            maxEnergyText.text = string.Format("{0}", max);
            fillRenderer.material.SetFloat("_CurrentHp", resource);
            fillRenderer.material.SetFloat("_MaxHp", Math.Max(max, resource));

            //don't play for first energy, and only play for resources increasing
            if (max > 1 && prevResource < resource)
            {
                sounds.PlaySound("getEnergy");
            }
            prevResource = resource;
        }

        internal void updatePreview(float maxEn, float turnLength)
        {
            maxEnergy = maxEn;
            timerAccum = 0f;
            this.turnLengthMs = turnLength;
            animatingEnergy = true;
            playedIncomingTurn = false;
            fillRendererPreview.material.SetFloat("_MaxHp", maxEnergy);
        }

        internal void setOn(bool on)
        {
            animatingEnergy = on;
        }
    }
}

