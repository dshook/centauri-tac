using strange.extensions.command.impl;
using ctac.signals;
using System;

namespace ctac
{
    public class TryRegisterCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public Credentials creds { get; set; }

        public override void Execute()
        {
            register(creds.username, creds.password);
        }

        private void register(string user, string password)
        {
            socket.Request(Guid.NewGuid(), "auth", "register",
                new
                {
                    email = user,
                    password = password
                }
            );
        }
    }
}

