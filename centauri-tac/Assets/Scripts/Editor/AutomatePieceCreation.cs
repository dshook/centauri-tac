using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ctac;
using ctac.util;

class AutomatePieceCreation : EditorWindow
{
    [MenuItem("Window/AutomatePieceCreation")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(AutomatePieceCreation));
        window.Show();
    }

    int pieceId;
    int pieceStartCreateId;
    void OnGUI()
    {
        pieceId = EditorGUILayout.IntField("Piece Template Id: ", pieceId);
        pieceStartCreateId = EditorGUILayout.IntField("Piece Create Start Id: ", pieceStartCreateId);

        if (GUILayout.Button("Save"))
        {
            var loader = new ResourceLoaderService();
            string newAssetPath = "Assets/Resources/Models/" + pieceStartCreateId;
            string newTexturePath = "Models/chibi_female/Textures/cz_f_58";
            System.IO.Directory.CreateDirectory(newAssetPath);

            var pieceModelResource = loader.Load<GameObject>("Models/" + pieceId + "/prefab");
            if(pieceModelResource == null){ Debug.LogError("Template Piece Not Found"); return; }

            // var material = loader.Load<Material>("Models/" + pieceId + "/material");
            // if(material == null){ Debug.LogError("Template Material Not Found"); return; }

            var material = new Material(Shader.Find("Custom/Piece"));
            material.color = ColorExtensions.HexToColor("969696FF");

            var texture = loader.Load<Texture>(newTexturePath);
            if(material == null){ Debug.LogError("Template Texture Not Found"); return; }

            var rampTexture = loader.Load<Texture>("Images/ToonGradient");

            material.mainTexture = texture;
            material.SetTexture("_Ramp", rampTexture);

            AssetDatabase.CreateAsset(material, newAssetPath + "/material.mat");

            var meshRenderer = pieceModelResource.GetComponentInChildren<SkinnedMeshRenderer>();
            meshRenderer.material = material;

            // Save the transform's GameObject as a prefab asset.
            PrefabUtility.CreatePrefab(newAssetPath + "/prefab.prefab", pieceModelResource);

            Debug.Log(string.Format("Created " + pieceStartCreateId));
        }
    }

}