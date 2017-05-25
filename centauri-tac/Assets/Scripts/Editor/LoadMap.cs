using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using ctac;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ctac.signals;

class LoadMap : EditorWindow
{
    [MenuItem("Window/LoadMap")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(LoadMap));
        window.Show();
    }

    string levelName;
    void OnGUI()
    {
        try
        {
            levelName = EditorGUILayout.TextField("Level Name: ", levelName);

            if (GUILayout.Button("Load"))
            {
                GameObject contextGO = GameObject.Find("Context");
                //DI not available in editor so fake it all
                var mapCreator = new MapCreatorService();
                mapCreator.contextView = contextGO;
                mapCreator.mapCreated = new MapCreatedSignal();
                mapCreator.mapModel = new MapModel();

                //fetch map from disk, eventually comes from server
                string mapContents = File.ReadAllText(string.Format("../maps/{0}.json", levelName));

                var mapModel = JsonConvert.DeserializeObject<MapImportModel>(mapContents);
                mapCreator.CreateMap(mapModel);

            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not load map " + e);
        }
    }

}