using strange.extensions.command.impl;
using ctac.signals;
using System;

namespace ctac
{
    public class TryLoginCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public Credentials creds { get; set; }

        public override void Execute()
        {
            login(creds.username, creds.password);
        }

        private void login(string user, string password)
        {
            socket.Request(Guid.NewGuid(), "auth", "login",
                new
                {
                    email = user,
                    password = password
                }
            );
        }
    }
}

