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
            mim.startingPositions = new List<TileImportPosition>();

            for (int t = 0; t < mapGO.transform.childCount; t++)
            {
                var tile = mapGO.transform.GetChild(t);
                if (tile.name == "Props")
                {
                    mim.props = GetProps(tile.gameObject);
                    continue;
                }
                if (tile.name == "Tiles")
                {
                    GetTiles(tile.gameObject, mim.tiles, mim.startingPositions);
                }
            }

            File.WriteAllText("../maps/" + levelName + ".json", JsonConvert.SerializeObject(mim, Formatting.Indented) );
            Debug.Log("Saved map " + levelName);
        }
    }

    List<PropImport> GetProps(GameObject propRoot)
    {
        var props = new List<PropImport>();

        for (int t = 0; t < propRoot.transform.childCount; t++)
        {
            var prop = propRoot.transform.GetChild(t);
            var mesh = prop.GetComponent<MeshFilter>();
            var rotation = prop.transform.rotation.eulerAngles;
            props.Add(new PropImport()
            {
                transform = new PropImportPosition() {
                    x = prop.transform.position.x,
                    y = prop.transform.position.y,
                    z = prop.transform.position.z
                },
                rotation = new PropImportPosition() {
                    x = rotation.x,
                    y = rotation.y,
                    z = rotation.z
                },
                //ghetto way for now to find out what prop this is but works
                propName = mesh.sharedMesh.name
            });
        }

        return props;
    }

    void GetTiles(GameObject tileRoot, List<TileImport> tiles, List<TileImportPosition> startingPositions)
    {
            for (int t = 0; t < tileRoot.transform.childCount; t++)
            {
                var tile = tileRoot.transform.GetChild(t);

                var tileView = tile.GetComponent<TileView>();
                if (tileView == null) continue;

                var meshRenderer = tile.GetChild(0).GetComponent<MeshRenderer>();
                var matName = meshRenderer.sharedMaterial.name.Replace("tile_", "").Replace(" (Instance)", "");
                var tiPosition = new TileImportPosition() {
                    x = (int)tile.transform.position.x,
                    y = tile.transform.position.y,
                    z = (int)tile.transform.position.z
                };

                tiles.Add(new TileImport()
                {
                    transform = tiPosition,
                    material = matName,
                    unpassable = tileView.unpassable || matName == "water"
                });

                if (tileView.isStartTile)
                {
                    startingPositions.Add(tiPosition);
                }
            }

    }

}