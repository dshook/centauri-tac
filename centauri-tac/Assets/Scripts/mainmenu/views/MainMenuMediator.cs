using strange.extensions.mediation.impl;
using UnityEngine;

namespace ctac
{
    public class MainMenuMediator : Mediator
    {
        [Inject] public MainMenuView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        bool startSettled = false;

        public override void OnRegister()
        {
            view.clickPlaySignal.AddListener(onPlayClicked);
            view.clickCardsSignal.AddListener(onCardsClicked);
            view.clickOptionsSignal.AddListener(onOptionsClicked);
            view.clickLeaveSignal.AddListener(onLeaveClicked);

            view.init();
        }

        public override void onRemove()
        {
            view.clickPlaySignal.RemoveListener(onPlayClicked);
            view.clickCardsSignal.RemoveListener(onCardsClicked);
            view.clickOptionsSignal.RemoveListener(onOptionsClicked);
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);
        }

        public void Update()
        {
        }

        private void onPlayClicked()
        {
        }

        private void onCardsClicked()
        {
        }

        private void onOptionsClicked()
        {
        }

        private void onLeaveClicked()
        {
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            Application.Quit();
        }
    }
}

