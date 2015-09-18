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
        [Inject]
        public SocketReRequestSignal reRequest { get; set; }

        private WebSocket ws = null;
        private bool connected = false;

        private MonoBehaviour root;

        [PostConstruct]
        public void PostConstruct()
        {
            root = contextView.GetComponent<SignalsRoot>();
            reRequest.AddListener(Request);
        }

        private IEnumerator ConnectAndRequest(string componentName, string methodName, object data)
        {
            yield return root.StartCoroutine(SocketConnect(componentName));
           
            yield return root.StartCoroutine(MakeRequest(componentName, methodName, data));
        }

        public void Request(string componentName, string methodName, object data = null)
        {
            if (connected)
            {
                connectSignal.RemoveAllListeners();
                root.StartCoroutine(MakeRequest(componentName, methodName, data));
            }
            else
            {
                root.StartCoroutine(ConnectAndRequest(componentName, methodName, data));
                //Connect(componentName);
                //connectSignal.AddListener(() => reRequest.Dispatch(componentName, methodName, data));
            }
        }

        private IEnumerator SocketConnect(string componentName)
        {
            var url = componentModel.getComponentWSURL(componentName);
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

            ws.ConnectAsync();
            int i = 0;
            while (i < 1000)
            {
                i++;
                if (connected) break;
                yield return null;
            }
        }

        private IEnumerator MakeRequest(string componentName, string methodName, object data)
        {
            if (!connected)
            {
                Debug.LogError("Trying to make a request to disconnected web socket");
                yield return null;
            }

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
            var signal = binder.GetInstance(signalType) as BaseSignal;
            if (signal != null)
            {
                var signalDataTypes = signal.GetTypes();
                if (signalDataTypes.Count != 1)
                {
                    Debug.LogError("Signal can only have one type of data to dispatch");
                    return;
                }
                var signalDataType = signalDataTypes[0];
                if (signalDataType.IsInterface)
                {
                    signalDataType = binder.GetInstance(signalDataType).GetType();
                }
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
            Debug.Log("Socket Error: " + e.Message + " " + e.Exception.Message);
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

