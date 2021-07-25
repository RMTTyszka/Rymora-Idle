using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragCamera2D))]
public class DragCamera2DInspector : Editor {

    DragCamera2D dc2d;

    private void OnEnable() {
        dc2d = (DragCamera2D)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Create Bounds")) {
            dc2d.addCameraBounds();
        }

        if (GUILayout.Button("Create Dolly")) {
            dc2d.addCameraDolly();
        }
    }
}
