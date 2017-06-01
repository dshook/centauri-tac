using UnityEngine;
using UnityEditor;
using System.IO;
using ctac;
using System.Collections.Generic;

class SaveRender : EditorWindow
{
    [MenuItem("Window/SaveRender")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(SaveRender));
        window.Show();
    }

    string pieceId;
    int resWidth = 500;
    int resHeight = 500;
    void OnGUI()
    {
        pieceId = EditorGUILayout.TextField("Piece Id: ", pieceId);

        if (GUILayout.Button("Save"))
        {
            var camera = Camera.main;
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            DestroyImmediate(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = Application.dataPath + "/Resources/Models/" + pieceId + "/render.png";
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
        }
    }

}