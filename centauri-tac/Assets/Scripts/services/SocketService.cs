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
        void Request(SocketKey key, string methodName, object data = null);
        void Request(Guid clientId, string componentName, string methodName, object data = null);

        void Disconnect(SocketKey key);
        void Disconnect(Guid clientId, string componentName);

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
            quit.AddListener(DestroySockets);
        }

        public void Request(Guid clientId, string componentName, string methodName, object data = null)
        {
            Request(new SocketKey(clientId, componentName), methodName, data);
        }

        public void Request(SocketKey key, string methodName, object data = null)
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
                root.StartCoroutine(ConnectAndRequest(key, methodName, data));
            }
        }

        private IEnumerator ConnectAndRequest(SocketKey key, string methodName, object data)
        {
            yield return root.StartCoroutine(SocketConnect(key));
           
            yield return root.StartCoroutine(MakeRequest(key, methodName, data));
        }

        private IEnumerator SocketConnect(SocketKey key)
        {
            var url = componentModel.getComponentWSURL(key.componentName);
            if (string.IsNullOrEmpty(url))
            {
                debug.LogError("Could not find url to open socket for component " + key.componentName, key);
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
                debug.LogError("Trying to make a request to disconnected web socket", key);
                yield return null;
            }

            string message = methodName + " " + JsonConvert.SerializeObject(data);
            debug.Log("Send: " + message, ErrorLevel.NetSend, key);
            ws.Send(message);

            yield return null;
        }

        private void onSocketMessage(SocketKey key, object sender, MessageEventArgs e)
        {
            debug.Log("Msg: " + e.Data, ErrorLevel.NetRecv, key);
            //chop it up and convert to appropriate signal based on header
            var delimiterIndex = e.Data.IndexOf(' ');
            string messageType = e.Data.Substring(0, delimiterIndex);
            string messageData = e.Data.Substring(delimiterIndex + 1);

            var signalType = typeMap.Get(messageType);
            if (signalType == null)
            {
                debug.LogWarning("No message type for " + messageType, key);
                return;
            }
            BaseSignal signal = null;
            try
            {
                signal = binder.GetInstance(signalType) as BaseSignal;
            }
            catch(Exception ex)
            {
                debug.LogError("Could not get get instance of signal for " + messageType + " " + signalType + " " + ex.ToString(), key);
                return;
            }
            if (signal != null)
            {
                var signalDataTypes = signal.GetTypes();
                bool attachKey = false;
                if(signalDataTypes.Count == 2 && signalDataTypes[1] == typeof(SocketKey)) {
                    attachKey = true;
                }
                else if (signalDataTypes.Count != 1)
                {
                    debug.LogError("Signal can only have one type of data to dispatch", key);
                    return;
                }
                var signalDataType = signalDataTypes[0];
                var deserializedData = JsonConvert.DeserializeObject(messageData, signalDataType);

                if (deserializedData == null)
                {
                    debug.LogWarning("Null data for " + messageType, key);
                    return;
                }

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
                debug.LogError("Could not find signal to dispatch from message type " + messageType, key);
            }
        }

        private void onSocketError(SocketKey key, object sender, ErrorEventArgs e) {
            debug.Log("Socket Error: " + e.Message + " " + e.Exception.Message + "\n" + e.Exception.StackTrace, key);
            errorSignal.Dispatch(key, e.Message);
        }

        private void onSocketOpen(SocketKey key, object sender, EventArgs e) {
            debug.Log("Socket Open For " + key.clientId.ToShort() + " " + key.componentName, key);
            connectSignal.Dispatch(key);
        }

        private void onSocketClose(SocketKey key, object sender, CloseEventArgs e) {
            debug.Log("Socket Close: " + key.clientId.ToShort() + " " + key.componentName + " " + e.Reason, key);
            disconnectSignal.Dispatch(key);
        }

        public void Disconnect(Guid clientId, string componentName)
        {
            Disconnect(new SocketKey(clientId, componentName));
        }

        public void Disconnect(SocketKey key)
        {
            var ws = sockets.Get(key);
            if (ws == null)
            {
                debug.LogWarning("Trying to disconnect from disconnected socket", key);
            }

            ws.Close(CloseStatusCode.Normal, "Requested Disconnect");
            sockets.Remove(key);
        }

        void DestroySockets()
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

