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

    int textureStartNumber;
    int textureEndNumber;
    void OnGUI()
    {
        pieceId = EditorGUILayout.IntField("Piece Template Id: ", pieceId);
        pieceStartCreateId = EditorGUILayout.IntField("Piece Create Start Id: ", pieceStartCreateId);

        textureStartNumber = EditorGUILayout.IntField("Texture Start Number:", textureStartNumber);
        textureEndNumber = EditorGUILayout.IntField("Texture End Number", textureEndNumber);

        if (GUILayout.Button("Save"))
        {
            if(textureStartNumber > textureEndNumber){
                Debug.LogError("Texture numbers are fucked");
                return;
            }
            var loader = new ResourceLoaderService();

            for(var textureNumber = textureStartNumber; textureNumber <= textureEndNumber; textureNumber++){
                string newAssetPath = "Assets/Resources/Models/" + pieceStartCreateId;
                string newTexturePath = "Models/chibi_male/Textures/cz_m_" + textureNumber.ToString("00");
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
                pieceStartCreateId++;
            }
        }
    }

}