using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace ctac
{
    public class JsonNetworkService : IJsonNetworkService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; } 

        [Inject]
        public IAuthModel authModel { get; set; }

        [Inject]
        public FulfillWebServiceRequestSignal fulfillSignal { get; set; }

        private string componentName;
        private string methodName;
        private Dictionary<string, string> data;
        private Type type;

        public void Request(string componentName, string methodName, Type type, Dictionary<string, string> data = null)
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
            var headers = new Dictionary<string, string>()
            {
                {"accept", "application / json"}
            };

            WWWForm form = new WWWForm();
            if (data != null) {
                foreach (var key in data.Keys) {
                    form.AddField(key, data[key]);
                }
            }
            //for whatever reason, you have to add something to the form for it to be considered a POST request
            //but not having anything doesn't work as a GET request
            form.AddField("z", "z");

            if (!string.IsNullOrEmpty(authModel.token)) {
                headers.Add("authorization", "Bearer " + authModel.token );
            }

            WWW www = new WWW(url, form.data, headers);
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

