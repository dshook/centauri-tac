using UnityEngine.UI;
using strange.extensions.mediation.impl;
using UnityEngine;
using TMPro;

namespace ctac
{
    public class PlayMenuView : View
    {
        public Button leaveButton;
        public Button playButton;
        public GameObject deckHolder;

        public TextMeshProUGUI queueText;
        private TextMeshProUGUI playText;

        internal void init()
        {
            playText = playButton.GetComponentInChildren<TextMeshProUGUI>();

            //disable till deck is selected
            playButton.interactable = false;
        }

        float accum = 0;
        void Update()
        {
            accum += Time.deltaTime;

            if (queueing)
            {
                if (accum > 1f)
                {
                    accum = 0f;
                    //Queueing = 8 characters
                    queueText.text += ".";
                    if (queueText.text.Length > 28)
                    {
                        queueText.text = "Queuing\n";
                    }
                }
            }
        }

        internal bool queueing = false;
        internal void SetQueueing(bool status)
        {
            queueing = status;
            if (status)
            {
                setMessage("Queuing\n");
                playText.text = "Stop";
                deckHolder.SetActive(false);
            }
            else
            {
                setMessage("");
                playText.text = "Play";
                deckHolder.SetActive(true);
            }
        }

        internal void SetLoadingProgress(float scaledPerc)
        {
            //disable play button and 
            queueing = false;

            setMessage("Loading " + (100f * scaledPerc).ToString("F0"));

            if (scaledPerc >= 1f)
            {
                setMessage("Loading Complete");
            }
        }

        internal void setMessage(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                queueText.gameObject.SetActive(false);
            }
            else
            {
                queueText.gameObject.SetActive(true);
            }
            queueText.text = text;
        }

        internal void SetButtonsActive(bool active)
        {
            playButton.interactable = active;
            leaveButton.interactable = active;
        }
    }
}

