using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraBounds))]
public class CameraBoundsInspector : Editor
{
    CameraBounds cb;
    private Rect boundArea;

    private const float wpSize = 0.1f;

    private void OnEnable() {
        cb = (CameraBounds)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }

    private void OnSceneGUI() {
        float zoom = HandleUtility.GetHandleSize(new Vector3(0, 0, 0)); // basically gets a scene view zoom level

        boundArea.x = cb.transform.position.x;
        boundArea.y = cb.transform.position.y;
        boundArea.width = cb.pointa.x - cb.transform.position.x;
        boundArea.height = cb.pointa.y - cb.transform.position.y;

        Handles.Label(cb.transform.position + new Vector3(0,-0.2f,0),"Bottom Left Boundary");
        Handles.Label(cb.pointa + new Vector3(0, -0.2f, 0), "Top Right Boundary");

        cb.pointa = Handles.Slider2D(cb.pointa, Vector3.forward, Vector3.right, Vector3.up, wpSize * zoom, Handles.CircleHandleCap, 0.1f);
        cb.transform.position = Handles.Slider2D(cb.transform.position, Vector3.forward, Vector3.right, Vector3.up, wpSize * zoom, Handles.CircleHandleCap, 0.1f);

        if (cb.pointa.x < cb.transform.position.x) {
            cb.pointa.x = cb.transform.position.x;
        }

        if (cb.pointa.y < cb.transform.position.y) {
            cb.pointa.y = cb.transform.position.y;
        }

        Vector3 pos = cb.transform.position;

        Vector3[] verts = new Vector3[]
        {
            cb.transform.position,
            cb.transform.position + new Vector3(0, boundArea.height, 0),
            cb.transform.position + new Vector3(boundArea.width, boundArea.height, 0),
            cb.transform.position + new Vector3(boundArea.width,0 , 0)
        };

        Handles.DrawSolidRectangleWithOutline(verts, cb.guiColour, Color.white);
    }

}
