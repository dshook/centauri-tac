using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;
using System.IO;
using Newtonsoft.Json;

namespace ctac
{
    public class StartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public StartConnectSignal startConnect { get; set; }

        [Inject]
        public IMapCreatorService mapCreator { get; set; }

        public override void Execute()
        {
            startConnect.Dispatch();

            //fetch map from disk, eventually comes from server
            string mapContents = File.ReadAllText("../maps/cubeland.json");
            var defaultMap = JsonConvert.DeserializeObject<MapModel>(mapContents);

            mapCreator.CreateMap(defaultMap);
        }
    }
}

