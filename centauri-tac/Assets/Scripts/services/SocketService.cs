using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace ctac
{
    public interface ISocketService
    {
        void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null);

        SocketMessage messageSignal { get; }
    }

    public class SocketService : ISocketService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; } 

        [Inject]
        public IAuthModel authModel { get; set; }

        [Inject]
        public SocketMessage messageSignal { get; set; }
        [Inject]
        public SocketError errorSignal { get; set; }
        [Inject]
        public SocketDisconnect disconnectSignal { get; set; }


        public void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null)
        {
            MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
            root.StartCoroutine(MakeRequest(componentName, methodName, type, data));
        }

        private IEnumerator Connect()
        {
            yield return null;
        }

        private IEnumerator MakeRequest( string componentName, string methodName, Type type, Dictionary<string, string> data)
        {
            var url = componentModel.getComponentURL(componentName) + "/" + methodName;
            var headers = new Dictionary<string, string>()
            {
                {"accept", "application / json"}
            };

            if (!string.IsNullOrEmpty(authModel.token)) {
                headers.Add("authorization", "Bearer " + authModel.token );
            }

            using (var ws = new WebSocket("ws://dragonsnest.far/Laputa"))
            {
                ws.OnMessage += onSocketMessage;
                ws.OnError += onSocketError;
                ws.OnOpen += onSocketOpen;
                ws.OnClose += onSocketClose;

                ws.Connect();
                ws.Send("BALUS");
            }

            yield return null;
        }

        private void onSocketMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Socket Message: " + e.Data);
            messageSignal.Dispatch(e.Data);
        }

        private void onSocketError(object sender, ErrorEventArgs e) {
            Console.WriteLine("Socket Error: " + e.Message);
            errorSignal.Dispatch(e.Message);
        }

        private void onSocketOpen(object sender, EventArgs e) {
            Console.WriteLine("Socket Open");
            
        }

        private void onSocketClose(object sender, CloseEventArgs e) {
            Console.WriteLine("Socket Close: " + e.Reason);

            disconnectSignal.Dispatch();
        }
    }
}

