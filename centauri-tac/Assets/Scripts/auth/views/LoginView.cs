using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using ctac.util;
using TMPro;

namespace ctac
{
    public class LoginView : View
    {
        public Signal loginClickSignal = new Signal();
        public Signal registerClickSignal = new Signal();

        public GameObject holder;
        public GameObject loginPanel;
        public GameObject registerPanel;

        public TMP_InputField loginEmail;
        public TMP_InputField loginPassword;
        public Button openRegisterationButton;
        public Button loginButton;

        public TMP_InputField registerEmail;
        public TMP_InputField registerPassword;
        public TMP_InputField registerPasswordConfirm;
        public Button cancelRegistrationButton;
        public Button registerButton;
        public TextMeshProUGUI message;

        //char[] passwordChars = new char[]{'$', '%', '!', '@', '#', '^', '&', '*', '(', ')', '-', '_', '+', '='};
        //System.Random r = new System.Random();
        //float updateFreq = 0.5f;
        //float updateTimer = 0f;

        Color buttonTextColor;
        TextMeshProUGUI buttonText;
        //TextMeshProUGUI passwordPlaceholderText;
        Color newButtonTextColor;

        internal void init()
        {
            loginButton.onClick.AddListener(() => loginClickSignal.Dispatch());
            registerButton.onClick.AddListener(() => registerClickSignal.Dispatch());

            openRegisterationButton.onClick.AddListener(onOpenRegisterClick);
            cancelRegistrationButton.onClick.AddListener(onCancelRegisterClick);

            buttonText = loginButton.GetComponentInChildren<TextMeshProUGUI>();
            //passwordPlaceholderText = password.placeholder.GetComponent<TextMeshProUGUI>();
            buttonTextColor = buttonText.color;
            newButtonTextColor = buttonTextColor;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if(registerPanel.activeSelf){
                    registerClickSignal.Dispatch();
                }else if(loginPanel.activeSelf){
                    loginClickSignal.Dispatch();
                }
            }

            //updateTimer += Time.deltaTime;
            //if (updateTimer > updateFreq)
            //{
            //    updateTimer = 0;
            //    var length = 13;
            //    r.Shuffle(passwordChars);
            //    passwordPlaceholderText.text = new string(passwordChars.Take(length).ToArray());
            //}

            buttonText.color = Color.Lerp(newButtonTextColor, buttonTextColor, 2f * Time.deltaTime);
            newButtonTextColor = buttonText.color;
        }

        public void onBadPassword(string userMessage)
        {
            newButtonTextColor = new Color(0.6f, 0.1f, 0.1f);
            setMessage(userMessage);
        }

        public void setMessage(string userMessage){
            message.text = userMessage;
            message.color = Color.white;
            iTween.ColorTo(message.gameObject, Color.clear, 6f);
        }

        public void onOpenRegisterClick(){
            registerPanel.SetActive(true);
            loginPanel.SetActive(false);
        }

        public void onCancelRegisterClick(){
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
    }
}

