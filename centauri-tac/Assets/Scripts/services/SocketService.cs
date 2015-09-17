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
        public SocketConnectSignal connectSignal { get; set; }
        [Inject]
        public SocketMessageSignal messageSignal { get; set; }
        [Inject]
        public SocketErrorSignal errorSignal { get; set; }
        [Inject]
        public SocketDisconnectSignal disconnectSignal { get; set; }

        private WebSocket ws = null;
        private bool connected = false;

        public void Connect(string url)
        {
            MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
            root.StartCoroutine(SocketConnect(url));
        }

        public void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null)
        {
            MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
            root.StartCoroutine(MakeRequest(componentName, methodName, type, data));
        }

        private IEnumerator SocketConnect(string url)
        {
            ws = new WebSocket(url);

            ws.WaitTime = new TimeSpan(0, 0, 5);
            ws.OnMessage += onSocketMessage;
            ws.OnError += onSocketError;
            ws.OnOpen += onSocketOpen;
            ws.OnClose += onSocketClose;

            ws.Connect();

            yield return null;
        }

        private IEnumerator MakeRequest( string componentName, string methodName, Type type, Dictionary<string, string> data)
        {
            if (!connected)
            {
                Debug.LogError("Trying to make a request to disconnected web socket");
                yield return null;
            }
            var url = componentModel.getComponentURL(componentName) + "/" + methodName;
            var headers = new Dictionary<string, string>()
            {
                {"accept", "application / json"}
            };

            if (!string.IsNullOrEmpty(authModel.token)) {
                headers.Add("authorization", "Bearer " + authModel.token );
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
            connected = true;
            connectSignal.Dispatch();
        }

        private void onSocketClose(object sender, CloseEventArgs e) {
            Console.WriteLine("Socket Close: " + e.Reason);
            connected = false;
            disconnectSignal.Dispatch();
        }
    }
}

