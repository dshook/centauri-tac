using strange.extensions.mediation.impl;
using System;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class PlayerResourceView : View
    {
        public bool isOpponent;
        public GameObject turnCountdown = null;

        TextMeshProUGUI currentEnergyText;
        TextMeshProUGUI maxEnergyText;
        MeshRenderer fillRenderer;
        MeshRenderer fillRendererPreview;

        RectTransform turnCountdownProgressRect = null;

        ISoundService sounds;

        float turnLengthMs = 0f;
        float turnEndBufferLengthMs = 0f;

        float timerAccumMs = 0f;
        bool animatingEnergy = false;
        float maxEnergy = 0f;

        bool playedIncomingTurn = false;

        internal void init(ISoundService s)
        {
            currentEnergyText = transform.Find("CurrentEnergyText").GetComponent<TextMeshProUGUI>();
            maxEnergyText = transform.Find("MaxEnergyText").GetComponent<TextMeshProUGUI>();

            fillRenderer = transform.Find("EnergyBarFill").GetComponent<MeshRenderer>();
            fillRendererPreview = transform.Find("EnergyBarFillPreview").GetComponent<MeshRenderer>();

            if(turnCountdown != null){
                turnCountdownProgressRect = turnCountdown.transform.Find("progress fill").GetComponent<RectTransform>();
            }

            sounds = s;
        }

        void Update()
        {
            if(!animatingEnergy || turnLengthMs == 0f) return;

            timerAccumMs += Time.deltaTime * 1000f;

            //Start playing a turn finished sound when the end turn buffer starts for us
            var isInEndBuffer = timerAccumMs > (turnLengthMs - turnEndBufferLengthMs);
            if (!isOpponent && !playedIncomingTurn && isInEndBuffer)
            {
                sounds.PlaySound("turnFinish");
                playedIncomingTurn = true;
            }

            if(turnCountdown != null){
                if(isInEndBuffer)
                {
                    turnCountdown.SetActive(true);
                    //what % are we through the end turn buffer, inversed
                    var inverseProgress = Mathf.Clamp(1 - (timerAccumMs - (turnLengthMs - turnEndBufferLengthMs)) / turnEndBufferLengthMs, 0, 1f);
                    turnCountdownProgressRect.localScale = turnCountdownProgressRect.localScale.SetX(inverseProgress);
                }
                else
                {
                    turnCountdown.SetActive(false);
                }
            }
            
            var progress = Math.Min(maxEnergy, maxEnergy * (timerAccumMs / turnLengthMs));
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
            timerAccumMs = 0f;
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

