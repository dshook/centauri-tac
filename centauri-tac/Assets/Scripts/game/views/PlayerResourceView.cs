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

        float turnLengthMs = 0f;
        float turnEndBufferLengthMs = 0f;

        float timerAccum = 0f;
        bool animatingEnergy = false;
        float maxEnergy = 0f;

        bool playedIncomingTurn = false;

        internal void init(ISoundService s)
        {
            currentEnergyText = transform.FindChild("CurrentEnergyText").GetComponent<TextMeshProUGUI>();
            maxEnergyText = transform.FindChild("MaxEnergyText").GetComponent<TextMeshProUGUI>();

            fillRenderer = transform.FindChild("EnergyBarFill").GetComponent<MeshRenderer>();
            fillRendererPreview = transform.FindChild("EnergyBarFillPreview").GetComponent<MeshRenderer>();
            sounds = s;
        }

        void Update()
        {
            if(!animatingEnergy || turnLengthMs == 0f) return;

            timerAccum += Time.deltaTime;

            //Start playing a turn finished sound when the end turn buffer starts
            if (!playedIncomingTurn && timerAccum > (turnLengthMs - turnEndBufferLengthMs) / 1000f)
            {
                sounds.PlaySound("turnFinish");
                playedIncomingTurn = true;
            }
            
            var progress = Math.Min(maxEnergy, maxEnergy * ((timerAccum * 1000f) / turnLengthMs));
            fillRendererPreview.material.SetFloat("_CurrentHp", progress);
        }

        int prevResource = 0;
        internal void setEnergy(int resource, int max, bool playSound)
        {
            if (currentEnergyText == null) { return; }

            currentEnergyText.text = string.Format("{0}", resource);
            maxEnergyText.text = string.Format("{0}", max);
            fillRenderer.material.SetFloat("_CurrentHp", resource);
            fillRenderer.material.SetFloat("_MaxHp", Math.Max(max, resource));

            //don't play for first energy, and only play for resources increasing
            if (playSound && max > 1 && prevResource <= resource)
            {
                sounds.PlaySound("getEnergy");
            }
            prevResource = resource;
        }

        internal void updatePreview(float maxEn)
        {
            maxEnergy = maxEn;
            timerAccum = 0f;
            animatingEnergy = true;
            playedIncomingTurn = false;
            fillRendererPreview.material.SetFloat("_MaxHp", maxEnergy);
        }

        internal void setOn(bool on)
        {
            animatingEnergy = on;
        }

        internal void setTimers(float turnLen, float turnBuffer)
        {
            turnLengthMs = turnLen;
            turnEndBufferLengthMs = turnBuffer;
        }
    }
}

