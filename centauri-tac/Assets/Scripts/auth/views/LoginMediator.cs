using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LoginMediator : Mediator
    {
        [Inject] public LoginView view { get; set; }

        [Inject] public TryLoginSignal loginSignal { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLoginClicked);

            //wait for signal to wake up
            view.enabled = false;
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLoginClicked);
        }

        private void onLoginClicked()
        {
            loginSignal.Dispatch(new Credentials() { username = view.email.text, password = view.password.text });
        }

        [ListensTo(typeof(NeedLoginSignal))]
        private void onNeedLogin(string message)
        {
            view.enabled = true;
            view.holder.SetActive(true);
            if(!string.IsNullOrEmpty(message)){
                view.setMessage(message);
            }
            view.init();
        }

        [ListensTo(typeof(AuthLoggedInSignal))]
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

        [ListensTo(typeof(FailedAuthSignal))]
        public void onFailAuth()
        {
            view.onBadPassword("Nope, Try Again");
        }

    }
}

