using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ctac
{
    public interface IJsonNetworkService
    {
        void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null);
        //Instead of an EventDispatcher, we put the actual Signals into the Interface
        FulfillWebServiceRequestSignal fulfillSignal { get; }
    }

    public class JsonNetworkService : IJsonNetworkService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; } 

        [Inject]
        public FulfillWebServiceRequestSignal fulfillSignal { get; set; }


        public void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null)
        {
            MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
            root.StartCoroutine(MakeRequest(componentName, methodName, type, data));
        }

        private IEnumerator MakeRequest( string componentName, string methodName, Type type, Dictionary<string, string> data)
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
                Debug.LogError("Could not deserialize json " + e.Message);
            }
            fulfillSignal.Dispatch(url, ret);
        }
    }
}

