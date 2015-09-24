using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using strange.extensions.injector.api;
using strange.extensions.signal.impl;
using ctac.signals;

namespace ctac
{
    public interface ISocketService
    {
        void Request(SocketKey key, string methodName, object data = null, string url = null);
        void Request(Guid clientId, string componentName, string methodName, object data = null, string url = null);

        SocketMessageSignal messageSignal { get; set; }
    }

    public class SocketService : ISocketService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

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

        private Dictionary<SocketKey, WebSocket> sockets = new Dictionary<SocketKey, WebSocket>();

        private MonoBehaviour root;

        [PostConstruct]
        public void PostConstruct()
        {
            root = contextView.GetComponent<SignalsRoot>();
            quit.AddListener(DestroySocketService);
        }

        public void Request(Guid clientId, string componentName, string methodName, object data = null, string url = null)
        {
            Request(new SocketKey(clientId, componentName), methodName, data, url);
        }

        public void Request(SocketKey key, string methodName, object data = null, string url = null)
        {
            if (key == null)
            {
                debug.LogError("Cannot make request with a null SocketKey");
            }
            var ws = sockets.Get(key);
            if (ws != null)
            {
                connectSignal.RemoveAllListeners();
                root.StartCoroutine(MakeRequest(key, methodName, data));
            }
            else
            {
                root.StartCoroutine(ConnectAndRequest(key, methodName, data, url));
            }
        }

        private IEnumerator ConnectAndRequest(SocketKey key, string methodName, object data, string overrideUrl)
        {
            yield return root.StartCoroutine(SocketConnect(key, overrideUrl));
           
            yield return root.StartCoroutine(MakeRequest(key, methodName, data));
        }

        private IEnumerator SocketConnect(SocketKey key, string overrideUrl = null)
        {
            string url;
            if (!string.IsNullOrEmpty(overrideUrl))
            {
                url = overrideUrl;
            }
            else
            {
                url = componentModel.getComponentWSURL(key.componentName);
            }
            if (string.IsNullOrEmpty(url))
            {
                debug.LogError("Could not find url to open socket for component " + key.componentName);
                yield return null;
            }
            var ws = new WebSocket(url);
            sockets[key] = ws;

            ws.WaitTime = new TimeSpan(0, 0, 5);
            ws.OnMessage += (o, e) => onSocketMessage(key, o, e);
            ws.OnError += (o, e) => onSocketError(key, o, e);
            ws.OnOpen += (o, e) => onSocketOpen(key, o, e);
            ws.OnClose += (o, e) => onSocketClose(key, o, e);

            ws.ConnectAsync();
            int i = 0;
            while (i < 1000)
            {
                i++;
                if (ws.ReadyState == WebSocketState.Open) break;
                yield return null;
            }
        }

        private IEnumerator MakeRequest(SocketKey key, string methodName, object data)
        {
            var ws = sockets.Get(key);
            if (ws.ReadyState != WebSocketState.Open)
            {
                debug.LogError("Trying to make a request to disconnected web socket");
                yield return null;
            }

            string message = methodName + " " + JsonConvert.SerializeObject(data);
            ws.Send(message);

            yield return null;
        }

        private void onSocketMessage(SocketKey key, object sender, MessageEventArgs e)
        {
            //debug.Log(key.clientId.ToShort() + " " + key.componentName + " Msg: " + e.Data);
            //chop it up and convert to appropriate signal based on header
            var delimiterIndex = e.Data.IndexOf(' ');
            string messageType = e.Data.Substring(0, delimiterIndex);
            string messageData = e.Data.Substring(delimiterIndex + 1);

            var signalType = typeMap.Get(messageType);
            var signal = binder.GetInstance(signalType) as BaseSignal;
            if (signal != null)
            {
                var signalDataTypes = signal.GetTypes();
                bool attachKey = false;
                if(signalDataTypes.Count == 2 && signalDataTypes[1] == typeof(SocketKey)) {
                    attachKey = true;
                }
                else if (signalDataTypes.Count != 1)
                {
                    debug.LogError("Signal can only have one type of data to dispatch");
                    return;
                }
                var signalDataType = signalDataTypes[0];
                var deserializedData = JsonConvert.DeserializeObject(messageData, signalDataType);

                object[] signalData;
                if (attachKey)
                {
                    signalData = new object[] { deserializedData, key };
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
                debug.LogError("Could not find signal to dispatch from message type " + messageType);
            }
        }

        private void onSocketError(SocketKey key, object sender, ErrorEventArgs e) {
            debug.Log("Socket Error: " + e.Message + " " + e.Exception.Message);
            errorSignal.Dispatch(key, e.Message);
        }

        private void onSocketOpen(SocketKey key, object sender, EventArgs e) {
            debug.Log("Socket Open For " + key.clientId.ToShort() + " " + key.componentName);
            connectSignal.Dispatch(key);
        }

        private void onSocketClose(SocketKey key, object sender, CloseEventArgs e) {
            debug.Log("Socket Close: " + key.clientId.ToShort() + " " + key.componentName + " " + e.Reason);
            disconnectSignal.Dispatch(key);
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

