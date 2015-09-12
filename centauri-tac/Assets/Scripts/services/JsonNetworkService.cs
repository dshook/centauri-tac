using System.Collections;
using UnityEngine;
using strange.extensions.context.api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

            WWWForm form = null;
            if (data != null) {
                form = new WWWForm();
                foreach (var key in data.Keys) {
                    form.AddField(key, data[key]);
                }
            }
            if (!string.IsNullOrEmpty(authModel.token)){
                form.AddField("auth", JsonConvert.SerializeObject(new { bearer = authModel.token }));
            }
            WWW www;
            if (form != null)
            {
                www = new WWW(url, form);
            }
            else
            {
                www = new WWW(url);
            }
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

