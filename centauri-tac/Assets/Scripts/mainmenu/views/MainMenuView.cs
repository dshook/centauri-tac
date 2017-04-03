using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class MainMenuView : View
    {
        public Signal clickPlaySignal = new Signal();
        public Signal clickCardsSignal = new Signal();
        public Signal clickOptionsSignal = new Signal();
        public Signal clickLeaveSignal = new Signal();

        public Button playButton;
        public Button cardsButton;
        public Button optionsButton;
        public Button leaveButton;

        public TextMeshProUGUI username;
        public TextMeshProUGUI queueText;

        public TextMeshProUGUI playText;
        public TextMeshProUGUI cardsText;
        public TextMeshProUGUI optionsText;

        internal void init()
        {
            playButton.onClick.AddListener(() => clickPlaySignal.Dispatch());
            cardsButton.onClick.AddListener(() => clickCardsSignal.Dispatch());
            optionsButton.onClick.AddListener(() => clickOptionsSignal.Dispatch());
            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());

            playText = playButton.GetComponentInChildren<TextMeshProUGUI>();
            cardsText = cardsButton.GetComponentInChildren<TextMeshProUGUI>();
            optionsText = optionsButton.GetComponentInChildren<TextMeshProUGUI>();

            username.text = "";

            //disable buttons initially until the login is settled
            playButton.interactable = false;
            cardsButton.interactable = false;
            optionsButton.interactable = false;

            playText.color = playButton.colors.disabledColor;
            cardsText.color = cardsButton.colors.disabledColor;
            optionsText.color = optionsButton.colors.disabledColor;
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
                    if (queueText.text.Length > 18)
                    {
                        queueText.text = "Queueing";
                    }
                }
            }
        }

        internal void SetButtonsActive(bool active)
        {
            playButton.gameObject.SetActive(active);
            cardsButton.gameObject.SetActive(active);
            optionsButton.gameObject.SetActive(active);
            leaveButton.gameObject.SetActive(active);
        }

        internal void SetUsername(string text)
        {
            username.text = text;
        }

        internal void enableButtons()
        {
            playButton.interactable = true;
            //cardsButton.interactable = true;
            //optionsButton.interactable = true;

            playText.color = playButton.colors.normalColor;
            //cardsText.color = cardsButton.colors.normalColor;
            //optionsText.color = optionsButton.colors.normalColor;
        }

        internal bool queueing = false;
        internal void SetQueueing(bool status)
        {
            queueing = status;
            if (status)
            {
                queueText.gameObject.SetActive(true);
                playText.text = "Stop";
            }
            else
            {
                queueText.gameObject.SetActive(false);
                playText.text = "Play";
            }
        }
    }
}

