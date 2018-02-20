using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LoginMediator : Mediator
    {
        [Inject] public LoginView view { get; set; }

        [Inject] public TryLoginSignal loginSignal { get; set; }
        [Inject] public TryRegisterSignal registerSignal { get; set; }

        public override void OnRegister()
        {
            view.loginClickSignal.AddListener(onLoginClicked);
            view.registerClickSignal.AddListener(onRegisterClicked);

            //wait for signal to wake up
            view.enabled = false;
        }

        public override void OnRemove()
        {
            view.loginClickSignal.RemoveListener(onLoginClicked);
            view.registerClickSignal.RemoveListener(onRegisterClicked);
        }

        private void onLoginClicked()
        {
            if(string.IsNullOrEmpty(view.loginEmail.text) || string.IsNullOrEmpty(view.loginPassword.text)){
                view.onBadPassword("Please enter both email and password");
                return;
            }
            loginSignal.Dispatch(new Credentials() { username = view.loginEmail.text, password = view.loginPassword.text });
        }

        private void onRegisterClicked()
        {
            if(string.IsNullOrEmpty(view.registerEmail.text) || string.IsNullOrEmpty(view.registerPassword.text)){
                view.onBadPassword("Please enter both email and password");
                return;
            }
            if(view.registerPassword.text != view.registerPasswordConfirm.text){
                view.onBadPassword("Passwords don't match");
                return;
            }
            registerSignal.Dispatch(new Credentials() { username = view.registerEmail.text, password = view.registerPassword.text });
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

                view.loginEmail.text = "";
                view.loginPassword.text = "";
            }
            else
            {
                view.onBadPassword(status.message);
            }
        }

        [ListensTo(typeof(RegisteredSignal))]
        private void onRegistered(LoginStatusModel status, SocketKey key)
        {
            if (status.status)
            {
                view.setMessage(status.message);
                view.loginEmail.text = view.registerEmail.text;
                view.onCancelRegisterClick();
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

