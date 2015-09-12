using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LoginMediator : Mediator
    {
        [Inject]
        public LoginView view { get; set; }

        [Inject]
        public TryLoginSignal loginSignal { get; set; }

        [Inject]
        public LoggedInSignal loggedInSignal { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLoginClicked);
            view.init();

            loggedInSignal.AddListener(onLoggedIn);
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLoginClicked);
        }

        private void onLoginClicked()
        {
            Debug.Log("Login");
            loginSignal.Dispatch(view.email.text, view.password.text);
        }

        private void onLoggedIn(IAuthModel auth)
        {
            view.enabled = false;
            view.gameObject.SetActive(false);
        }
    }
}

