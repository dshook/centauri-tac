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
        public Signal clickAboutSignal = new Signal();
        public Signal clickLeaveSignal = new Signal();
        public Signal clickLogoutSignal = new Signal();

        public Button playButton;
        public Button cardsButton;
        public Button optionsButton;
        public Button aboutButton;
        public Button leaveButton;
        public Button logoutButton;

        public GameObject usernamePanel;
        public TextMeshProUGUI username;
        public TextMeshProUGUI queueText;

        private TextMeshProUGUI playText;
        private TextMeshProUGUI cardsText;
        private TextMeshProUGUI optionsText;
        private TextMeshProUGUI aboutText;

        internal void init()
        {
            playButton.onClick.AddListener(() => clickPlaySignal.Dispatch());
            cardsButton.onClick.AddListener(() => clickCardsSignal.Dispatch());
            optionsButton.onClick.AddListener(() => clickOptionsSignal.Dispatch());
            aboutButton.onClick.AddListener(() => clickAboutSignal.Dispatch());
            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
            logoutButton.onClick.AddListener(() => clickLogoutSignal.Dispatch());

            playText = playButton.GetComponentInChildren<TextMeshProUGUI>();
            cardsText = cardsButton.GetComponentInChildren<TextMeshProUGUI>();
            optionsText = optionsButton.GetComponentInChildren<TextMeshProUGUI>();
            aboutText = aboutButton.GetComponentInChildren<TextMeshProUGUI>();

            username.text = "";
            usernamePanel.SetActive(false);

            //disable buttons initially until the login is settled
            playButton.interactable = false;
            cardsButton.interactable = false;
            optionsButton.interactable = false;
            aboutButton.interactable = false;

            playText.color = playButton.colors.disabledColor;
            cardsText.color = cardsButton.colors.disabledColor;
            optionsText.color = optionsButton.colors.disabledColor;
            aboutText.color = aboutButton.colors.disabledColor;
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
            aboutButton.gameObject.SetActive(active);
            leaveButton.gameObject.SetActive(active);
        }

        internal void SetUsername(string text)
        {
            username.text = text;
            usernamePanel.SetActive(!string.IsNullOrEmpty(text));
        }

        internal void enableButtons()
        {
            playButton.interactable = true;
            cardsButton.interactable = true;
            //optionsButton.interactable = true;

            playText.color = playButton.colors.normalColor;
            cardsText.color = cardsButton.colors.normalColor;
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

        internal void SetLoadingProgress(float scaledPerc)
        {
            //disable play button and 
            queueing = false;

            queueText.gameObject.SetActive(true);
            queueText.text = "Loading " + (100f * scaledPerc).ToString("F0");

            if (scaledPerc >= 1f)
            {
                queueText.text = "Loading Complete";
            }
        }
    }
}

