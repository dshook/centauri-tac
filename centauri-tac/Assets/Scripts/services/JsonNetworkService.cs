using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;

namespace ctac
{
    public class JsonNetworkService : IJsonNetworkService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; } 

        [Inject]
        public FulfillWebServiceRequestSignal fulfillSignal { get; set; }

        private string componentName;
        private string methodName;
        private object data;
        private Type type;

        public void Request(string componentName, string methodName, Type type, object data = null)
        {
            this.componentName = componentName;
            this.methodName = methodName;
            this.data = data;
            this.type = type;

            MonoBehaviour root = contextView.GetComponent<SignalsRoot>();
            root.StartCoroutine(MakeRequest());
        }

        private IEnumerator MakeRequest()
        {
            var url = componentModel.getComponentURL(componentName) + "/" + methodName;
            WWW www = new WWW(url);
            yield return www;

            object ret = null;
            try
            {
                ret = JsonConvert.DeserializeObject(www.text, type);
                //ret = new System.Object();
            }
            catch (Exception e)
            {
                Debug.LogError("Could not deserialize json " + e.Message);
            }
            //Pass back some fake data via a Signal
            fulfillSignal.Dispatch(url, ret);
        }
    }
}

