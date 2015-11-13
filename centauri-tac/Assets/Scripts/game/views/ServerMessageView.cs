using strange.extensions.mediation.impl;
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

        internal void updateText(string message)
        {
            serverText.text = message;
            serverText.color = Color.white;
            iTweenExtensions.ColorUpdate(gameObject, transparentWhite, 2f);
        }
    }
}

