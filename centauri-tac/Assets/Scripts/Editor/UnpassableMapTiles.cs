using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using ctac;
using System.Collections.Generic;
using Newtonsoft.Json;

class UnpassableMapTiles : EditorWindow
{
    [MenuItem("Window/UnpassableMapTiles")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(UnpassableMapTiles));
        window.Show();
    }

    string levelName;
    void OnGUI()
    {
        var mapGO = GameObject.Find("Map");
        var tileGO = mapGO.transform.FindChild("Tiles");

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

}