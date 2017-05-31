using System.Collections;
using UnityEngine;
using UnityEditor;

class FixBot : EditorWindow
{
    [MenuItem("Window/FixBot")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(FixBot));
        window.Show();
    }

    void OnGUI()
    {
        GameObject thisObject = Selection.activeObject as GameObject;
        if (thisObject == null)
        {
            return;
        }

        if (GUILayout.Button("Fix"))
        {
            var prefab = thisObject.transform.Find("c-bot_c_Prefab");
            if (prefab == null)
            {
                Debug.LogWarning("Couldn't find prefab");
                return;
            }
            thisObject.tag = "Piece";

            for (var c = 0; c < prefab.childCount; c++)
            {
                var child = prefab.GetChild(c);
                if (child.name == "Root_Bone") continue;
                
                var existingCollider = child.gameObject.GetComponent<MeshCollider>();
                if (existingCollider != null) continue;

                var mesh = child.gameObject.GetComponent<SkinnedMeshRenderer>();
                var collider = child.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = mesh.sharedMesh;
            }
        }

    }

}