using System.Collections.Generic;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;
using System;
using System.Linq;
using Newtonsoft.Json;
using strange.extensions.injector.api;

namespace ctac
{
    public class SignalData
    {
        public SocketKey key { get; set; }
        public string messageType { get; set; }
        public string messageData { get; set; }
    }

    /// <summary>
    /// This service takes signals that have been generated from the socket service on a different thread
    /// and then dispatches on the main unity thread so that anyone listening them can use them as normal
    /// </summary>
    public class SignalDispatcherService : View
    {
        private Queue<SignalData> queue = new Queue<SignalData>();
        private object _queueLock = new object();

        [Inject]
        public MessageToSignalModel typeMap { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public ICrossContextInjectionBinder binder { get; set; }

        void Update()
        {
            lock (_queueLock)
            {
                if (queue.Count > 0)
                {
                    var signalData = queue.Dequeue();
                    Dispatch(signalData.messageType, signalData.messageData, signalData.key);
                }
            }
        }

        public void ScheduleSignal(SignalData signalData)
        {
            lock (_queueLock)
            {
                queue.Enqueue(signalData);
            }
        }

        public void Dispatch(string messageType, string messageData, SocketKey key)
        {
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
                object deserializedData = null;
                try{
                    deserializedData = JsonConvert.DeserializeObject(messageData, signalDataType);
                }catch(Exception e){
                    var msg = e.ToString();
                    //just get the first line since the rest is noise
                    msg = msg.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                    debug.LogError(
                        string.Format("Error deserializing message {0}\n{1}", signalDataType.Name, msg)
                        , key
                    );
                    throw e;
                }

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

                var methodInfo = signalType
                             .GetMethods()
                             .Where(m => m.Name == "Dispatch")
                             .First();
                methodInfo.Invoke(signal, signalData );
            }
            else
            {
                debug.LogError("Could not find signal to dispatch from message type " + messageType, key);
            }
        }
    }
}
