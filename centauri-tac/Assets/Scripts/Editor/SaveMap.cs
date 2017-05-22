using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using ctac;
using System.Collections.Generic;
using Newtonsoft.Json;

class SaveMap : EditorWindow
{
    [MenuItem("Window/SaveMap")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        //var window = new SaveMap();
        //window.autoRepaintOnSceneChange = true;
        //window.Show();

        EditorWindow window = GetWindow(typeof(SaveMap));
        window.Show();
    }

    string levelName;
    void OnGUI()
    {
        levelName = EditorGUILayout.TextField("Level Name: ", levelName);

        if (GUILayout.Button("Save"))
        {
            var mapGO = GameObject.Find("Map");

            var mim = new MapImportModel();
            mim.name = levelName;
            mim.maxPlayers = 2;
            mim.tiles = new List<TileImport>();

            for (int t = 0; t < mapGO.transform.childCount; t++)
            {
                var tile = mapGO.transform.GetChild(t);
                var meshRenderer = tile.GetChild(0).GetComponent<MeshRenderer>();
                var matName = meshRenderer.sharedMaterial.name.Replace("tile_", "").Replace(" (Instance)", "");

                mim.tiles.Add(new TileImport()
                {
                    transform = new TileImportPosition() {
                        x = (int)tile.transform.position.x,
                        y = tile.transform.position.y,
                        z = (int)tile.transform.position.z
                    },
                    material = matName,
                    unpassable = matName == "water"
                });
            }

            File.WriteAllText("../maps/" + levelName + ".json", JsonConvert.SerializeObject(mim, Formatting.Indented) );
            
        }
    }

}