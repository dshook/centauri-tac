using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using ctac.signals;

namespace ctac
{
    public interface IDebugService
    {
        void Log(object message, SocketKey key = null);
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

 
        public void Log(object message, SocketKey key = null)
        {
            entries.Add(new LogEntry()
            {
                timestamp = DateTime.Now,
                level = ErrorLevel.info,
                key = key,
                message = MsgFmt(message)
            });
            Debug.Log(KeyFmt(key) + message);
        }
        public void LogWarning(object message, SocketKey key = null)
        {
            entries.Add(new LogEntry()
            {
                timestamp = DateTime.Now,
                level = ErrorLevel.warning,
                key = key,
                message = MsgFmt(message)
            });
            Debug.LogWarning(KeyFmt(key) + message);
        }
        public void LogError(object message, SocketKey key = null)
        {
            entries.Add(new LogEntry()
            {
                timestamp = DateTime.Now,
                level = ErrorLevel.error,
                key = key,
                message = MsgFmt(message)
            });
            Debug.LogError(KeyFmt(key) + message);
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
        public ErrorLevel level { get; set; }
        public SocketKey key { get; set; }
        public object message { get; set; }
    }

    internal enum ErrorLevel
    {
        info,
        warning,
        error
    }
}

