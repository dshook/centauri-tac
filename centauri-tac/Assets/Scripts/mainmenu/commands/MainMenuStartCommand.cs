using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;
using ctac.signals;
using System.IO;
using Newtonsoft.Json;
using System;

namespace ctac
{
    public class MainMenuStartCommand : Command
    {
        [Inject] public ServerAuthSignal serverAuthSignal { get; set; }
        [Inject] public ConfigModel config { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            debug.Log("Starting main menu");

            //override config from settings on disk if needed
            try
            {
                string configContents = File.ReadAllText("./config.json");
                if (!string.IsNullOrEmpty(configContents))
                {
                    debug.Log("Reading Config File");
                    var diskConfig = JsonConvert.DeserializeObject<ConfigModel>(configContents);
                    diskConfig.CopyProperties(config);
                }
            }
            catch (Exception)
            {
                debug.LogWarning("Couldn't read config file, using defaults");
                config.baseUrl = "http://ctac.herokuapp.com/";
            }

            serverAuthSignal.Dispatch();
        }
    }
}

