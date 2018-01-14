using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using ctac;
using System.Collections.Generic;
using Newtonsoft.Json;
using ctac.signals;
using System;

class MapTools : EditorWindow
{
    [MenuItem("Window/MapTools")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        //var window = new SaveMap();
        //window.autoRepaintOnSceneChange = true;
        //window.Show();

        EditorWindow window = GetWindow(typeof(MapTools));
        window.Show();
    }

    string levelName;
    void OnGUI()
    {
        levelName = EditorGUILayout.TextField("Level Name: ", levelName);

        if (GUILayout.Button("Save"))
        {
            SaveMap();
        }

        DrawSeparator(2);

        if (GUILayout.Button("Load"))
        {
            try
            {
                LoadMap();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not load map " + e);
            }
        }

        DrawSeparator();

        GUILayout.Label("Utils");

        if (GUILayout.Button("Select Unpassable Tiles"))
        {
            SelectUnpassable();
        }
        if (GUILayout.Button("Select Clearable Tiles"))
        {
            SelectClearable();
        }
        if (GUILayout.Button("Select Breakable Props"))
        {
            SelectBreakable();
        }

        if (GUILayout.Button("Fix Tile Positions"))
        {
            FixTilePositions();
        }
    }

    void DrawSeparator(int num = 4)
    {
        for (int i = 0; i < num; i++)
        {
            EditorGUILayout.Space();
        }
    }

    void SaveMap()
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

    List<PropImport> GetProps(GameObject propRoot)
    {
        var props = new List<PropImport>();

        for (int t = 0; t < propRoot.transform.childCount; t++)
        {
            var prop = propRoot.transform.GetChild(t);
            var mesh = prop.GetComponent<MeshFilter>();
            var view = prop.GetComponent<PropView>();
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
                propName = mesh.sharedMesh.name,
                breakable = view.breakable
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
                unpassable = tileView.unpassable || matName == "water",
                clearable = tileView.clearable
            });

            if (tileView.isStartTile)
            {
                startingPositions.Add(tiPosition);
            }
        }
    }

    void LoadMap()
    {
        GameObject contextGO = GameObject.Find("Context");
        //DI not available in editor so fake it all
        var mapCreator = new MapCreatorService();
        mapCreator.contextView = contextGO;
        mapCreator.mapCreated = new MapCreatedSignal();
        mapCreator.mapModel = new MapModel();
        mapCreator.loader = new ResourceLoaderService();

        //fetch map from disk, eventually comes from server
        string mapContents = File.ReadAllText(string.Format("../maps/{0}.json", levelName));

        var mapModel = JsonConvert.DeserializeObject<MapImportModel>(mapContents);
        mapCreator.CreateMap(mapModel);
    }

    void SelectUnpassable()
    {
        var mapGO = GameObject.Find("Map");
        var tileGO = mapGO.transform.Find("Tiles");

        var selected = new List<GameObject>();
        for (int t = 0; t < tileGO.childCount; t++)
        {
            var tile = tileGO.transform.GetChild(t);

            var tileView = tile.GetComponent<TileView>();
            if (tileView == null) continue;

            if (tileView.unpassable)
            {
                selected.Add(tile.gameObject);
            }
        }

        Selection.objects = selected.ToArray();
    }

    void SelectClearable()
    {
        var mapGO = GameObject.Find("Map");
        var tileGO = mapGO.transform.Find("Tiles");

        var selected = new List<GameObject>();
        for (int t = 0; t < tileGO.childCount; t++)
        {
            var tile = tileGO.transform.GetChild(t);

            var tileView = tile.GetComponent<TileView>();
            if (tileView == null) continue;

            if (tileView.clearable)
            {
                selected.Add(tile.gameObject);
            }
        }

        Selection.objects = selected.ToArray();
    }

    void SelectBreakable()
    {
        var mapGO = GameObject.Find("Map");
        var tileGO = mapGO.transform.Find("Props");

        var selected = new List<GameObject>();
        for (int t = 0; t < tileGO.childCount; t++)
        {
            var tile = tileGO.transform.GetChild(t);

            var propView = tile.GetComponent<PropView>();
            if (propView == null) continue;

            if (propView.breakable)
            {
                selected.Add(tile.gameObject);
            }
        }

        Selection.objects = selected.ToArray();
    }

    /// <summary>
    /// when drag selecting tiles then moving them around the cube's are getting moved and not the actual tile
    /// which is ultimately the fault of the drag select not selecting the right thing. 
    /// This function grabs all the cubes and makes sure their local position is 0 and the tile is in the right position
    /// </summary>
    void FixTilePositions()
    {
        var mapGO = GameObject.Find("Map");
        var tileGO = mapGO.transform.Find("Tiles");

        for (int t = 0; t < tileGO.childCount; t++)
        {
            var tile = tileGO.transform.GetChild(t);

            var cube = tile.Find("cube");

            tile.transform.position = cube.transform.position + new Vector3(0, 0.5f, 0);
            cube.localPosition = new Vector3(0, -0.5f, 0);

            var collider = tile.GetComponent<BoxCollider>();
            collider.center = new Vector3(0, -0.5f, 0);

        }
    }

}