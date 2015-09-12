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
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public LoggedInSignal loggedInSignal { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLoginClicked);
            view.init();

            failedAuth.AddListener(OnFailAuth);
            loggedInSignal.AddListener(onLoggedIn);
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLoginClicked);
        }

        public void OnFailAuth()
        {
            view.onBadPassword();
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

