using UnityEngine;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;
using System;
using System.Linq;

namespace ctac
{

    public class SignalData
    {
        public BaseSignal signal { get; set; }
        public Type signalType { get; set; }
        public object[] signalData { get; set; }
    }

    /// <summary>
    /// This service takes signals that have been generated from the socket service on a different thread
    /// and then dispatches on the main unity thread so that anyone listening them can use them as normal 
    /// </summary>
    public class SignalDispatcherService : View
    {
        private Queue<SignalData> queue = new Queue<SignalData>();
        private object _queueLock = new object();

        // Update is called once per frame
        void Update()
        {
            lock (_queueLock)
            {
                if (queue.Count > 0)
                {
                    var signalData = queue.Dequeue();
                    Dispatch(signalData.signal, signalData.signalType, signalData.signalData);
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

        public void Dispatch(object self, Type innerType, object[] data)
        {
            var methodInfo = innerType
                         .GetMethods()
                         .Where(m => m.Name == "Dispatch")
                         .First();
            methodInfo.Invoke(self, data );
        }
    }
}
