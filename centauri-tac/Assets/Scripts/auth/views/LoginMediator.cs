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

            //wait for signal to wake up
            view.enabled = false;
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLoginClicked);

            needLoginSignal.RemoveListener(onNeedLogin);
            failedAuth.RemoveListener(onFailAuth);
            loggedInSignal.RemoveListener(onLoggedIn);
        }

        public void onFailAuth()
        {
            view.onBadPassword("Nope, Try Again");
        }

        private void onLoginClicked()
        {
            loginSignal.Dispatch(new Credentials() { username = view.email.text, password = view.password.text });
        }

        private void onNeedLogin()
        {
            view.enabled = true;
            view.holder.SetActive(true);
            view.init();
        }

        private void onLoggedIn(LoginStatusModel status, SocketKey key)
        {
            if (status.status)
            {
                view.enabled = false;
                view.holder.SetActive(false);

                view.email.text = "";
                view.password.text = "";
            }
            else
            {
                view.onBadPassword(status.message);
            }
        }
    }
}

