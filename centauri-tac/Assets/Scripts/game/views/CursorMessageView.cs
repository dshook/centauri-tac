using strange.extensions.mediation.impl;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class CursorMessageView : View
    {
        public TextMeshProUGUI serverText;

        internal void init()
        {
            serverText = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
        }

        internal void updateText(string message, float? time)
        {
            if(string.IsNullOrEmpty(message)){
                serverText.enabled = false;
            }else{
                serverText.enabled = true;
            }

            serverText.text = message;
            serverText.color = Color.white;

        }
    }
}

