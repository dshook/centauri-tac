using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ctac
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CardsMessageView : View
    {
        [Inject] public CardsMenuMessageSignal cardsMessage { get; set; }

        private TextMeshProUGUI serverText;

        protected override void Awake()
        {
            base.Awake();
            serverText = GetComponent<TextMeshProUGUI>();
            cardsMessage.AddListener(updateText);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            cardsMessage.RemoveListener(updateText);
        }

        void Update()
        {
        }

        internal void updateText(string message)
        {
            var existingTween = gameObject.GetComponent<iTween>();
            if (existingTween != null)
            {
                Destroy(existingTween);
            }

            serverText.text = message;
            serverText.color = Color.white;

            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("from", Color.white);
            tweenParams.Add("to", (Color)Colors.transparentWhite);
            tweenParams.Add("time", 1);
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

