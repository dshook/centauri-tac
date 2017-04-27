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

        public override void OnRegister()
        {
            view.clickLeaveSignal.AddListener(onLeaveClicked);

            view.init();
        }

        public override void onRemove()
        {
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);
        }

        public void Update()
        {
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

