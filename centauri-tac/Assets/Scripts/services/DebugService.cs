using UnityEngine;

namespace ctac
{
    public interface IDebugService
    {
        void Log(object message);
        void LogFormat(string format, params object[] args);
        void LogError(object message);
        void LogErrorFormat(string format, params object[] args);
        void LogWarning(object message);
        void LogWarningFormat(string format, params object[] args);
    }

    public class UnityDebugService : IDebugService
    {
        public void Log(object message)
        {
            Debug.Log(message);
        }
        public void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }
        public void LogError(object message)
        {
            Debug.LogError(message);
        }
        public void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
        }
        public void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }
        public void LogWarningFormat(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }           
    }
}

