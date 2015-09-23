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
        public NeedLoginSignal needLoginSignal { get; set; }

        [Inject]
        public TryLoginSignal loginSignal { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public AuthLoggedInSignal loggedInSignal { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLoginClicked);

            needLoginSignal.AddListener(onNeedLogin);
            failedAuth.AddListener(onFailAuth);
            loggedInSignal.AddListener(onLoggedIn);
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onLoginClicked);
        }

        public void onFailAuth()
        {
            view.onBadPassword();
        }

        private void onLoginClicked()
        {
            loginSignal.Dispatch(new Credentials() { username = view.email.text, password = view.password.text });
        }

        private void onNeedLogin()
        {
            view.enabled = true;
            view.gameObject.SetActive(true);
            view.init();
        }

        private void onLoggedIn(LoginStatusModel status, SocketKey key)
        {
            view.enabled = false;
            view.gameObject.SetActive(false);

            needLoginSignal.RemoveListener(onNeedLogin);
            failedAuth.RemoveListener(onFailAuth);
            loggedInSignal.RemoveListener(onLoggedIn);
        }
    }
}
