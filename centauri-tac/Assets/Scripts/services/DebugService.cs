using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using ctac.signals;
using Newtonsoft.Json.Converters;

namespace ctac
{
    public interface IDebugService
    {
        void Log(object message, SocketKey key = null);
        void Log(object message, ErrorLevel level, SocketKey key = null);
        void LogWarning(object message, SocketKey key = null);
        void LogError(object message, SocketKey key = null);
    }

    public class UnityDebugService : IDebugService
    {
        [Inject]
        public QuitSignal quit { get; set; }

        private List<LogEntry> entries = new List<LogEntry>();

        [PostConstruct]
        public void PostConstruct()
        {
            quit.AddListener(Dump);
        }
        private object _logLock = new object();

        public void Log(object message, ErrorLevel level, SocketKey key = null)
        {
            lock (_logLock)
            {
                entries.Add(new LogEntry()
                {
                    timestamp = DateTime.Now,
                    level = level,
                    key = key,
                    message = MsgFmt(message)
                });
            }

            switch (level)
            {
                case ErrorLevel.Info:
                    Debug.Log(KeyFmt(key) + message);
                    break;
                case ErrorLevel.Warning:
                    Debug.LogWarning(KeyFmt(key) + message);
                    break;
                case ErrorLevel.Error:
                    Debug.LogError(KeyFmt(key) + message);
                    break;
            }

        }

        public void Log(object message, SocketKey key = null)
        {
            Log(message, ErrorLevel.Info, key);
        }

        public void LogWarning(object message, SocketKey key = null)
        {
            Log(message, ErrorLevel.Warning, key);
        }
        public void LogError(object message, SocketKey key = null)
        {
            Log(message, ErrorLevel.Error, key);
        }

        //when you just gotta go
        public void Dump()
        {
            string path = "../client_log.json";
            File.WriteAllText(path, JsonConvert.SerializeObject(entries));
        }

        private string KeyFmt(SocketKey key)
        {
            if(key == null) return "";
            return key.clientId.ToShort() + " " + key.componentName + " ";
        }

        private string MsgFmt(object message)
        {
            if((message as string) != null)
            {
                return (string) message;
            }
            return JsonConvert.SerializeObject(message);
        }
    }

    class LogEntry
    {
        public DateTime timestamp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorLevel level { get; set; }
        public SocketKey key { get; set; }
        public object message { get; set; }
    }

    public enum ErrorLevel
    {
        NetSend,
        NetRecv,
        Info,
        Warning,
        Error
        
    }
}

