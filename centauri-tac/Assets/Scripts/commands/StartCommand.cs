using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ctac
{
    public class StartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public FetchComponentsSignal startConnect { get; set; }

        [Inject]
        public MinionsModel minionsModel { get; set; }

        [Inject]
        public IMapCreatorService mapCreator { get; set; }

        public override void Execute()
        {
            startConnect.Dispatch();

            //fetch map from disk, eventually comes from server
            string mapContents = File.ReadAllText("../maps/cubeland.json");
            var defaultMap = JsonConvert.DeserializeObject<MapImportModel>(mapContents);

            mapCreator.CreateMap(defaultMap);

            //add minion view scripts to the existing minions, eventually will be set up from server
            minionsModel.minions = new List<MinionModel>();
            var taggedMinions = GameObject.FindGameObjectsWithTag("Minion");
            foreach (var minion in taggedMinions)
            {
                var minionModel = new MinionModel()
                {
                    gameObject = minion
                };

                minion.GetComponent<MinionView>().minion = minionModel;
                minionsModel.minions.Add(minionModel);
             }
        }
    }
}

