using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class CardsMenuMediator : Mediator
    {
        [Inject] public CardsMenuView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public ICardService cardService { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }

        [Inject] public CardsKickoffSignal cardKickoff { get; set; }

        public override void OnRegister()
        {
            view.clickLeaveSignal.AddListener(onLeaveClicked);
            cardKickoff.AddListener(onKickoff);

            view.init(cardService, cardDirectory);
        }

        public override void onRemove()
        {
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);
            cardKickoff.RemoveListener(onKickoff);
        }

        public void Update()
        {
        }

        private void onKickoff()
        {
            view.UpdateCards();
        }

        private void onLeaveClicked()
        {
            StartCoroutine("LoadLevel", "main");
        }

        public IEnumerator LoadLevel(string level)
        {
            SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);

            yield return null;
        }
    }
}

