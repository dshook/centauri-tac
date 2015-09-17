using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using strange.extensions.injector.api;
using strange.extensions.signal.impl;

namespace ctac
{
    public interface ISocketService
    {
        void Request(string componentName, string methodName, object data = null);

        SocketMessageSignal messageSignal { get; set; }
    }

    public class SocketService : ISocketService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public ICrossContextInjectionBinder binder { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; } 

        [Inject]
        public IAuthModel authModel { get; set; }

        [Inject]
        public IServiceTypeMapModel typeMap { get; set; }

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

        private Signal<string, string, object> needRerequest = new Signal<string, string, object>();

        private void Connect(string componentName)
        {
            MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
            root.StartCoroutine(SocketConnect(componentName));
        }

        public void Request(string componentName, string methodName, object data = null)
        {
            if (connected)
            {
                connectSignal.RemoveAllListeners();
                MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
                root.StartCoroutine(MakeRequest(componentName, methodName, data));
            }
            else
            {
                Connect(componentName);
                connectSignal.AddListener(() => Request(componentName, methodName, data));
            }
        }

        private IEnumerator SocketConnect(string componentName)
        {
            var url = componentModel.getComponentURL(componentName);
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("Could not find url to open socket for component " + componentName);
                yield return null;
            }
            ws = new WebSocket(url);

            ws.WaitTime = new TimeSpan(0, 0, 5);
            ws.OnMessage += onSocketMessage;
            ws.OnError += onSocketError;
            ws.OnOpen += onSocketOpen;
            ws.OnClose += onSocketClose;

            ws.Connect();

            yield return null;
        }

        private IEnumerator MakeRequest(string componentName, string methodName, object data)
        {
            if (!connected)
            {
                Debug.LogError("Trying to make a request to disconnected web socket");
                yield return null;
            }
            var url = componentModel.getComponentURL(componentName) + "/" + methodName;

            string message = methodName + " " + JsonConvert.SerializeObject(data);
            ws.Send(message);

            yield return null;
        }

        private void onSocketMessage(object sender, MessageEventArgs e)
        {
            Debug.Log("Socket Message: " + e.Data);
            //chop it up and convert to appropriate signal based on header
            var delimiterIndex = e.Data.IndexOf(' ');
            string messageType = e.Data.Substring(0, delimiterIndex);
            string messageData = e.Data.Substring(delimiterIndex + 1);

            var signalType = typeMap.Get(messageType);
            var signal = binder.GetInstance(signalType) as Signal;
            if (signal != null)
            {
                var signalDataTypes = signal.GetTypes();
                if (signalDataTypes.Count != 1)
                {
                    Debug.LogError("Signal can only have one type of data to dispatch");
                    return;
                }
                var signalDataType = signalDataTypes[0];
                var deserializedData = JsonConvert.DeserializeObject(messageData, signalDataType);
                signal.Dispatch(new object[] { deserializedData });
            }
            else
            {
                Debug.LogError("Could not find signal to dispatch from message type " + messageType);
            }

            messageSignal.Dispatch(e.Data);
        }

        private void onSocketError(object sender, ErrorEventArgs e) {
            Debug.Log("Socket Error: " + e.Message);
            errorSignal.Dispatch(e.Message);
        }

        private void onSocketOpen(object sender, EventArgs e) {
            Debug.Log("Socket Open");
            connected = true;
            connectSignal.Dispatch();
        }

        private void onSocketClose(object sender, CloseEventArgs e) {
            Debug.Log("Socket Close: " + e.Reason);
            connected = false;
            disconnectSignal.Dispatch();
        }
    }
}

