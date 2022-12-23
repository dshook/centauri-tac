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
        //GameObject thisObject = Selection.activeObject as GameObject;
        var selected = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel | SelectionMode.Editable);
        EditorGUILayout.LabelField("Objects to fix: "+ selected.Length);
        //if (thisObject == null)
        //{
        //    return;
        //}

        if (GUILayout.Button("Fix"))
        {
            foreach (var sel in selected)
            {
                FixPiece(sel);
            }
        }

    }

    void FixPiece(GameObject piece)
    {
        if (piece == null) return;

        piece.tag = "Piece";
        piece.name = "prefab";

        ProcessTransform(piece.transform);
    }

    void ProcessTransform(Transform t) {
        for (var c = 0; c < t.childCount; c++)
        {
            var child = t.GetChild(c);

            if (child.childCount > 0)
            {
                ProcessTransform(child);
            }

            //shouldn't have a collider already
            var existingCollider = child.gameObject.GetComponent<MeshCollider>();
            if (existingCollider != null) continue;

            //but should have a mesh
            var mesh = child.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (mesh == null) continue;

            var collider = child.gameObject.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.sharedMesh = mesh.sharedMesh;
        }
    }

}