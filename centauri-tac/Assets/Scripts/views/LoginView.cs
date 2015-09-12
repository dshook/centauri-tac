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

        internal void init()
        {
            loginButton.onClick.AddListener(() => onClick());
        }

        void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer > updateFreq)
            {
                updateTimer = 0;
                var length = Random.Range(6, 10);
                r.Shuffle(passwordChars);
                password.placeholder.GetComponent<Text>().text = new string(passwordChars.Take(length).ToArray());
            }

        }

        void onClick()
        {
            clickSignal.Dispatch();
        }
    }
}

