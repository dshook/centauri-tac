using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ctac.signals;
using strange.extensions.signal.impl;

namespace ctac
{
    public interface IJsonNetworkService
    {
        void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null);
        void GetJson<T>(string url, Signal<T> doneLoading) where T : class;
        //Instead of an EventDispatcher, we put the actual Signals into the Interface
        FulfillWebServiceRequestSignal fulfillSignal { get; }
    }

    public class JsonNetworkService : IJsonNetworkService
    {
        [Inject(InjectionKeys.PersistentSignalsRoot)]
        public GameObject contextView { get; set; }

        [Inject]
        public ComponentModel componentModel { get; set; } 

        [Inject]
        public FulfillWebServiceRequestSignal fulfillSignal { get; set; }

        [Inject]
        public ConfigModel config { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null)
        {
            MonoBehaviour root = contextView.GetComponent<PersistentSignalsRoot>();
            root.StartCoroutine(MakeRequest(componentName, methodName, type, data));
        }

        private IEnumerator MakeRequest(string componentName, string methodName, Type type, Dictionary<string, string> data)
        {
            var url = componentModel.getComponentURL(componentName) + "/" + methodName;

            WWW www = new WWW(url);
            yield return www;

            object ret = null;
            try
            {
                ret = JsonConvert.DeserializeObject(www.text, type);
            }
            catch (Exception e)
            {
                debug.LogError("Could not deserialize json " + e.Message);
            }
            fulfillSignal.Dispatch(url, ret);
        }

        public void GetJson<T>(string url, Signal<T> doneLoading) where T : class
        {
            MonoBehaviour root = contextView.GetComponent<PersistentSignalsRoot>();
            root.StartCoroutine(GetJsonCo(url, doneLoading));
        }
        private IEnumerator GetJsonCo<T>(string url, Signal<T> doneLoading) where T : class
        {
            var fullUrl = config.baseUrl + url;
            WWW www = new WWW(fullUrl);

            yield return www;

            T ret = default(T);
            try
            {
                ret = JsonConvert.DeserializeObject(www.text, typeof(T)) as T;
            }
            catch (Exception e)
            {
                debug.LogError(string.Format("Could not deserialize json for {0}. Error: {1} ", fullUrl, e));
            }
            doneLoading.Dispatch(ret);
        }
    }
}

