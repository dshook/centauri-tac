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

        internal void init()
        {
            playButton.onClick.AddListener(() => clickPlaySignal.Dispatch());
            cardsButton.onClick.AddListener(() => clickCardsSignal.Dispatch());
            optionsButton.onClick.AddListener(() => clickOptionsSignal.Dispatch());
            aboutButton.onClick.AddListener(() => clickAboutSignal.Dispatch());
            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
            logoutButton.onClick.AddListener(() => clickLogoutSignal.Dispatch());

            playText = playButton.GetComponentInChildren<TextMeshProUGUI>();

            username.text = "";
            usernamePanel.SetActive(false);

            //disable buttons initially until the login is settled
            disableButtons();

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
                    if (queueText.text.Length > 68)
                    {
                        queueText.text = "Queuing\n";
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
        }
        internal void disableButtons()
        {
            playButton.interactable = false;
            cardsButton.interactable = false;
            optionsButton.interactable = false;
            aboutButton.interactable = false;
        }

        internal bool queueing = false;
        internal void SetQueueing(bool status)
        {
            queueing = status;
            if (status)
            {
                setMessage("Queuing");
                playText.text = "Stop";
            }
            else
            {
                setMessage("");
                playText.text = "Play";
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
    }
}

