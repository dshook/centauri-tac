using strange.extensions.mediation.impl;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class ServerMessageView : View
    {
        public TextMeshProUGUI serverText;

        private Color transparentWhite = new Color(1, 1, 1, 0);
        internal void init()
        {
            serverText = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
        }

        internal void updateText(string message, float time)
        {
            serverText.text = message;
            serverText.color = Color.white;

            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("from", Color.white);
            tweenParams.Add("to", transparentWhite);
            tweenParams.Add("time", time);
            tweenParams.Add("delay", 0.75f);
            tweenParams.Add("onupdate", "OnColorUpdated");

            iTween.ValueTo(serverText.gameObject, tweenParams);
        }

        private void OnColorUpdated(Color color)
        {
            serverText.color = color;
        }
    }
}

