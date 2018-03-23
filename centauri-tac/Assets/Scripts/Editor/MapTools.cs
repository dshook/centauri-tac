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
    int selectTileX;
    int selectTileZ;

    float randomMin = 0;
    float randomMax = 1;
    string materialName;

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
        if (GUILayout.Button("Select Parent Objects"))
        {
            SelectParentObjects();
        }

        if (GUILayout.Button("Fix Tile Positions"))
        {
            FixTilePositions();
        }

        GUILayout.Label("Select Tile By Position");

        selectTileX = EditorGUILayout.IntField("Tile X", selectTileX);
        selectTileZ = EditorGUILayout.IntField("Tile Z", selectTileZ);

        if(GUILayout.Button("Find Tile")){
            FindTileByPos(selectTileX, selectTileZ);
        }

        GUILayout.Label("Randomize tile height in Range");
        randomMin = EditorGUILayout.FloatField("Min", randomMin);
        randomMax = EditorGUILayout.FloatField("Max", randomMax);

        if(GUILayout.Button("Randomize")){
            RandomizeHeights(randomMin, randomMax);
        }

        GUILayout.Label("Set or Select Tile Materials");
        materialName = EditorGUILayout.TextField("Material", materialName);

        if(GUILayout.Button("Set Material")){
            SetMaterials(materialName);
        }
        if(GUILayout.Button("Select by Material")){
            SelectMaterials(materialName);
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
        var tiles = GetSelectedTiles();
        var selected = new List<GameObject>();
        foreach(var tile in tiles){
            if (tile.unpassable)
            {
                selected.Add(tile.gameObject);
            }
        }

        Selection.objects = selected.ToArray();
    }

    void SelectClearable()
    {
        var tiles = GetSelectedTiles();

        var selected = new List<GameObject>();
        foreach(var tile in tiles){
            if (tile.clearable)
            {
                selected.Add(tile.gameObject);
            }
        }

        Selection.objects = selected.ToArray();
    }

    void SelectBreakable()
    {
        var selected = new List<GameObject>();
        var tiles = GetSelectedTiles();
        foreach(var tile in tiles){
            var propView = tile.gameObject.GetComponent<PropView>();
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
        var tiles = GetSelectedTiles();
        foreach(var tile in tiles){
            var cube = tile.transform.Find("cube");

            tile.transform.position = cube.transform.position + new Vector3(0, 0.5f, 0);
            cube.localPosition = new Vector3(0, -0.5f, 0);

            var collider = tile.GetComponent<BoxCollider>();
            collider.center = new Vector3(0, -0.5f, 0);
        }
    }

    void FindTileByPos(int x, int z)
    {

        var selected = new List<GameObject>();
        var tiles = GetSelectedTiles();
        foreach(var tile in tiles){
            if ((int)tile.transform.position.x == x && (int)tile.transform.position.z == z)
            {
                selected.Add(tile.gameObject);
            }
        }

        Selection.objects = selected.ToArray();
    }

    void SelectParentObjects()
    {
        var selected = new List<GameObject>();
        for (int t = 0; t < Selection.objects.Length; t++)
        {
            var go = Selection.objects[t] as GameObject;
            if(go == null) continue;

            //don't select parent if we're already on the tile
            var tileView = go.GetComponent<TileView>();
            if(tileView != null) continue;

            selected.Add(go.transform.parent.gameObject);
        }

        Selection.objects = selected.ToArray();
    }

    void RandomizeHeights(float min, float max)
    {
        var tiles = GetSelectedTiles();
        foreach(var tile in tiles){
            var pos = tile.transform.position;
            tile.transform.position = new Vector3(pos.x, UnityEngine.Random.Range(min, max), pos.z);
        }
    }

    void SetMaterials(string mat)
    {
        var loader = new ResourceLoaderService();
        var loadedMat = loader.Load<Material>("Maps/Tiles/Materials/" + mat);
        if(loadedMat == null){
            Debug.LogWarning("Material Not found");
            return;
        }
        var tiles = GetSelectedTiles();
        foreach(var tile in tiles){
            var meshRenderer = tile.GetComponentInChildren<MeshRenderer>();
            meshRenderer.sharedMaterial = loadedMat;
        }
    }

    void SelectMaterials(string mat)
    {
        var selected = new List<GameObject>();
        var tiles = GetSelectedTiles();
        foreach(var tile in tiles){
            var meshRenderer = tile.GetComponentInChildren<MeshRenderer>();

            if(meshRenderer.sharedMaterial.name == mat){
                selected.Add(tile.gameObject);
            }
        }
        Selection.objects = selected.ToArray();
    }

    //If stuff is selected use that, otherwise get all tiles
    List<TileView> GetSelectedTiles(){
        var tiles = new List<TileView>();
        if(Selection.gameObjects.Length > 0){
            for(int i = 0; i < Selection.gameObjects.Length; i++){
                var selected = Selection.gameObjects[i];
                var tv  = selected.GetComponent<TileView>();
                if(tv == null) continue;

                tiles.Add(tv);
            }
        }else{
            var mapGO = GameObject.Find("Map");
            var tileGO = mapGO.transform.Find("Tiles");

            for (int t = 0; t < tileGO.childCount; t++)
            {
                var tile = tileGO.transform.GetChild(t);

                var tv = tile.GetComponent<TileView>();
                if (tv == null) continue;

                tiles.Add(tv);
            }
        }

        return tiles;
    }

}