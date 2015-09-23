using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using ctac.util;

namespace ctac
{
    public class LoginView : View
    {
        public Signal clickSignal = new Signal();

        public InputField email;
        public InputField password;
        public Button loginButton;

        char[] passwordChars = new char[]{'$', '%', '!', '@', '#', '^', '&', '*', '(', ')', '-', '_', '+', '='};
        System.Random r = new System.Random();
        float updateFreq = 0.5f;
        float updateTimer = 0f;

        Color buttonTextColor;
        Text buttonText;
        Color newButtonTextColor;

        internal void init()
        {
            loginButton.onClick.AddListener(() => onClick());
            buttonText = loginButton.GetComponentInChildren<Text>();
            buttonTextColor = buttonText.color;
            newButtonTextColor = buttonTextColor;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                onClick();
            }

            updateTimer += Time.deltaTime;
            if (updateTimer > updateFreq)
            {
                updateTimer = 0;
                var length = Random.Range(6, 10);
                r.Shuffle(passwordChars);
                password.placeholder.GetComponent<Text>().text = new string(passwordChars.Take(length).ToArray());
            }

            buttonText.color = Color.Lerp(newButtonTextColor, buttonTextColor, 2f * Time.deltaTime);
            newButtonTextColor = buttonText.color;
        }

        public void onBadPassword()
        {
            newButtonTextColor = new Color(0.6f, 0.1f, 0.1f);
        }

        void onClick()
        {
            clickSignal.Dispatch();
        }
    }
}

