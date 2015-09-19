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
        public ComponentModel componentModel { get; set; } 

        [Inject]
        public ServiceTypeMapModel typeMap { get; set; }

        [Inject]
        public SignalDispatcherService signalDispatcher { get; set; }

        [Inject]
        public SocketConnectSignal connectSignal { get; set; }
        [Inject]
        public SocketMessageSignal messageSignal { get; set; }
        [Inject]
        public SocketErrorSignal errorSignal { get; set; }
        [Inject]
        public SocketDisconnectSignal disconnectSignal { get; set; }


        [Inject]
        public QuitSignal quit { get; set; }

        private Dictionary< string, WebSocket> sockets = new Dictionary<string, WebSocket>();

        private MonoBehaviour root;

        [PostConstruct]
        public void PostConstruct()
        {
            root = contextView.GetComponent<SignalsRoot>();
            quit.AddListener(DestroySocketService);
        }

        public void Request(string componentName, string methodName, object data = null)
        {
            var ws = sockets.Get(componentName);
            if (ws != null)
            {
                connectSignal.RemoveAllListeners();
                root.StartCoroutine(MakeRequest(componentName, methodName, data));
            }
            else
            {
                root.StartCoroutine(ConnectAndRequest(componentName, methodName, data));
            }
        }

        private IEnumerator ConnectAndRequest(string componentName, string methodName, object data)
        {
            yield return root.StartCoroutine(SocketConnect(componentName));
           
            yield return root.StartCoroutine(MakeRequest(componentName, methodName, data));
        }

        private IEnumerator SocketConnect(string componentName)
        {
            var url = componentModel.getComponentWSURL(componentName);
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("Could not find url to open socket for component " + componentName);
                yield return null;
            }
            var ws = new WebSocket(url);
            sockets[componentName] = ws;

            ws.WaitTime = new TimeSpan(0, 0, 5);
            ws.OnMessage += (o, e) => onSocketMessage(componentName, o, e);
            ws.OnError += (o, e) => onSocketError(componentName, o, e);
            ws.OnOpen += (o, e) => onSocketOpen(componentName, o, e);
            ws.OnClose += (o, e) => onSocketClose(componentName, o, e);

            ws.ConnectAsync();
            int i = 0;
            while (i < 1000)
            {
                i++;
                if (ws.ReadyState == WebSocketState.Open) break;
                yield return null;
            }
        }

        private IEnumerator MakeRequest(string componentName, string methodName, object data)
        {
            var ws = sockets.Get(componentName);
            if (ws.ReadyState != WebSocketState.Open)
            {
                Debug.LogError("Trying to make a request to disconnected web socket");
                yield return null;
            }

            string message = methodName + " " + JsonConvert.SerializeObject(data);
            ws.Send(message);

            yield return null;
        }

        private void onSocketMessage(string componentName, object sender, MessageEventArgs e)
        {
            Debug.Log(componentName + " Message: " + e.Data);
            //chop it up and convert to appropriate signal based on header
            var delimiterIndex = e.Data.IndexOf(' ');
            string messageType = e.Data.Substring(0, delimiterIndex);
            string messageData = e.Data.Substring(delimiterIndex + 1);

            var signalType = typeMap.Get(messageType);
            var signal = binder.GetInstance(signalType) as BaseSignal;
            if (signal != null)
            {
                var signalDataTypes = signal.GetTypes();
                bool attachMethodName = false;
                if(signalDataTypes.Count == 2 && signalDataTypes[1] == typeof(string)) {
                    attachMethodName = true;
                }
                else if (signalDataTypes.Count != 1)
                {
                    Debug.LogError("Signal can only have one type of data to dispatch");
                    return;
                }
                var signalDataType = signalDataTypes[0];
                var deserializedData = JsonConvert.DeserializeObject(messageData, signalDataType);

                object[] signalData;
                if (attachMethodName)
                {
                    signalData = new object[] { deserializedData, componentName };
                }
                else
                {
                    signalData = new object[] { deserializedData };
                }

                signalDispatcher.ScheduleSignal(
                    new SignalData() {
                        signal = signal,
                        signalType = signalType,
                        signalData = signalData
                    }
                );
            }
            else
            {
                Debug.LogError("Could not find signal to dispatch from message type " + messageType);
            }
        }

        private void onSocketError(string componentName, object sender, ErrorEventArgs e) {
            Debug.Log("Socket Error: " + e.Message + " " + e.Exception.Message);
            errorSignal.Dispatch(componentName, e.Message);
        }

        private void onSocketOpen(string componentName, object sender, EventArgs e) {
            Debug.Log("Socket Open");
            connectSignal.Dispatch(componentName);
        }

        private void onSocketClose(string componentName, object sender, CloseEventArgs e) {
            Debug.Log("Socket Close: " + e.Reason);
            disconnectSignal.Dispatch(componentName);
        }

        void DestroySocketService()
        {
            foreach (var ws in sockets)
            {
                if (ws.Value.ReadyState == WebSocketState.Open || ws.Value.ReadyState == WebSocketState.Connecting)
                {
                    ws.Value.Close(CloseStatusCode.Normal, "Client shut down");
                }
            }
        }
    }
}

