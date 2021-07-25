using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Dc2dDolly))]
public class Dc2dDollyInspector : Editor
{
    Dc2dDolly dolly;

    private const float wpSize = 0.1f;
    private const float buttonSize = 0.04f;
    private const float coneSize = 0.35f;
    private const float scaleHandleSize = 1f;

    private void OnEnable() {
        dolly = (Dc2dDolly)target;
    }

    protected virtual void OnSceneGUI() {
        float zoom = HandleUtility.GetHandleSize(new Vector3(0,0,0)); // basically gets a scene view zoom level

        //create post waypoint button
        Handles.color = dolly.tangentGuideColor;


        for (int i = 0; i < dolly.allWaypoints.Length; i++) {
            Dc2dWaypoint wp = dolly.allWaypoints[i];

            // position handle
            Handles.color = dolly.waypointColor;
            wp.position = Handles.Slider2D(wp.position, Vector3.forward, Vector3.right, Vector3.up, wpSize * zoom, Handles.CircleHandleCap, 0.01f);


            if (i < dolly.allWaypoints.Length - 1) { // not last
                // calc legnth
                wp.arclength = dolly.FastArcLength(wp);

                // show handles to adjust time to next waypoint  (Pro Only)

                //show length to next 
                if (dolly.showLength) {
                Handles.Label(wp.position + (Vector3.up + Vector3.right * 2), "Length:" + dolly.FastArcLength(wp).ToString());
                }

                //show timeToNext
                if (dolly.showTimes) {
                    Handles.Label(wp.position + (Vector3.right * 2), "Next:" + wp.timeToNextWaypoint.ToString());
                }

                // tan1 handle
                Handles.color = dolly.tangentHandleColor;
                Vector3 tempt1 = Handles.Slider2D(wp.tanOne + wp.position, Vector3.forward, Vector3.right, Vector3.up, buttonSize * zoom, Handles.DotHandleCap, 0.01f) - wp.position;

                // constant Speed  (Pro Only)
                wp.tanOne = tempt1;
                

                if (wp.tanOne.x < 0) {
                    wp.tanOne.x = 0;
                }
                Handles.color = dolly.tangentGuideColor;
                Handles.DrawDottedLine(wp.position, wp.tanOne + wp.position, 4.0f);

                // tan2 handle
                Handles.color = dolly.tangentHandleColor;
                Vector3 tempt2 = Handles.Slider2D(wp.tanTwo + wp.endPosition, Vector3.forward, Vector3.right, Vector3.up, buttonSize * zoom, Handles.DotHandleCap, 0.01f) - wp.endPosition;

                //constant Speed  (Pro Only)
                wp.tanTwo = tempt2;
                

                if (wp.tanTwo.x > 0) {
                    wp.tanTwo.x = 0;
                }
                Handles.color = dolly.tangentGuideColor;
                Handles.DrawDottedLine(wp.endPosition, wp.tanTwo + wp.endPosition, 4.0f);

                // draw da curve
                Handles.DrawBezier(wp.position, wp.endPosition, wp.position + wp.tanOne, wp.endPosition + wp.tanTwo, dolly.dollyTrackColor, null, 5f);

            }
            if (i > 0) { // not first
                // update position of previous
                dolly.allWaypoints[i - 1].endPosition = wp.position;

                if (wp.position.x < dolly.allWaypoints[i - 1].position.x) {
                    wp.position.x = dolly.allWaypoints[i - 1].position.x; // no going past previous point
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Add Waypoint")) {
            dolly.addWaypointToEnd();
        }
        GUI.enabled = false;
        if (GUILayout.Button("Smooth All (Pro Only)")) {
            for (int i = 0; i < dolly.allWaypoints.Length -1; i++) {
                dolly.smoothWaypoint(dolly.allWaypoints[i], dolly.allWaypoints[i+1]);
            }
            forceRepaint();
        }
        if (GUILayout.Button("Straighten All (Pro Only)")) {
            for (int i = 0; i < dolly.allWaypoints.Length - 1; i++) {
                dolly.straightenWaypoint(dolly.allWaypoints[i], dolly.allWaypoints[i + 1]);
            }
            forceRepaint();
        }
        if (GUILayout.Button("Even Section Times (Pro Only)")) {
            dolly.evenWaypointTimes();
            forceRepaint();
        }
        GUI.enabled = true;
    }

    private void forceRepaint() {
        EditorWindow view = EditorWindow.GetWindow<SceneView>();
        view.Repaint();
    }
}
