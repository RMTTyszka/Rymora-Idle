using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dc2dDolly : MonoBehaviour
{
    public enum DollyMode {
        TrackObject,
        TrackTimeProOnly
    }

    public enum WaypointTimeMode {
        PerWaypointTime,
        PerTrackTime
    }

    public DollyMode mode = DollyMode.TrackObject ;

    [Tooltip("Lock camera to track or follow tracked object when outside track area.")]
    public bool lockOnTrack = false;

    public Dc2dWaypoint[] allWaypoints;

    [Header("Track Time options  (Pro Only)")]
    [Tooltip("Delay Track start in seconds(can use negatives to jump ahead on track)  (Pro Only)")]
    public float delayStart = 0f;
    [Tooltip("Set time per waypoint or set time for track  (Pro Only)")]
    public WaypointTimeMode timeMode = WaypointTimeMode.PerWaypointTime;
    [Tooltip("Set the time for the camera to traverse from the start to the end  (Pro Only)")]
    public float totalTrackTime = 10f;

    // pro only
    //[Header("Bezier & Smoothing")]
    //[Tooltip("Limit curves so camera stays at constant speed.  (Pro Only)")]
    //public bool constantSpeed = true;


    [Header("GUI Options")]
    [Tooltip("Show Length of curve to next waypoint")]
    public bool showLength = true;
    [Tooltip("Show Time to traverse curve to next waypoint")]
    public bool showTimes = true;
    // pro only
    //[Tooltip("Show Handles to adjust time to next waypoint")]
    //public bool showTimeAdjustHandles = true;
    [Tooltip("Show dolly track during gameplay.")]
    public bool renderDollyTrack = false;
    [Tooltip("Color of waypoints (Circles)")]
    public Color waypointColor = Color.white;
    [Tooltip("Color of handles that effect curvature (Small Squares)")]
    public Color tangentHandleColor = Color.white;
    [Tooltip("Color of hashed lines between parent")]
    public Color tangentGuideColor = Color.gray;
    [Tooltip("Color of waypoint connections")]
    public Color gizmoColor = Color.gray;
    [Tooltip("Color of Time Scale Handle")]
    public Color timeScaleHandleColor = Color.red;
    [Tooltip("Color of the Dolly track")]
    public Color dollyTrackColor = Color.red;
    

    // private
    private LineRenderer lineRenderer;
    private int accuracy = 20;
    private float onTrackTime = 0f;
    private bool trackRendererUpdated = false;
    private bool proOnlyMessage = false;

    private void Start() {
        if (!lineRenderer) {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerID = 0;
        lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        lineRenderer.material.color = dollyTrackColor * 0.1f;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material.SetColor("_TintColor", new Color(1, 1, 1, 0.25f));

    }

    private void Update() {
        onTrackTime += Time.deltaTime;
        if (renderDollyTrack && !trackRendererUpdated) {
            renderTrack();
        } else if(!renderDollyTrack){
            lineRenderer.enabled = false;
            trackRendererUpdated = false;
        }
    }

    private void renderTrack() {
        for (int j = 0; j < allWaypoints.Length - 1; j++) {
            for (int i = 1; i <= accuracy; i++) {
                float t = i / (float)accuracy;
                int nodeIndex = j * 3;
                Vector3 pixel = Dc2dUtils.CalculateCubicBezierPoint(t, allWaypoints[j].position, allWaypoints[j].position + allWaypoints[j].tanOne, allWaypoints[j].endPosition + allWaypoints[j].tanTwo, allWaypoints[j].endPosition);
                lineRenderer.positionCount = ((j * accuracy) + i);
                lineRenderer.SetPosition((j * accuracy) + (i - 1), pixel);
            }
        }
        lineRenderer.enabled = true;
        trackRendererUpdated = true;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < allWaypoints.Length-1; i++) {
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(allWaypoints[i].position, allWaypoints[i+1].position);       
        }
    }

    private Vector3 getBezierTarget(Vector3 start, Vector3 end) {
        Vector3 bezPoint = (start - end) * 0.5f;
        bezPoint = start + bezPoint;
        return bezPoint;
    }

    public void addWaypointToStart() {
        // pro only
    }

    public void addWaypointToEnd() {
        Dc2dWaypoint[] tempPoints = new Dc2dWaypoint[allWaypoints.Length + 1];

        for(int i = 0; i < allWaypoints.Length; i++) {
            tempPoints[i] = allWaypoints[i];
        }

        Dc2dWaypoint newpoint = new Dc2dWaypoint();
        newpoint.position = tempPoints[tempPoints.Length - 2].getPostSpawn();
        tempPoints[tempPoints.Length - 2].endPosition = newpoint.position;
        tempPoints[tempPoints.Length - 1] = newpoint;
        allWaypoints = tempPoints;

    }

    public void getTimeBasedPosition() {
        // pro only
    }

    public Vector3 getTrackedObjectBasedPosition(Vector3 trackedObject) {
        Vector3 reVector = Vector3.zero;
        bool pointFound = false;
        for (int i = 0; i < allWaypoints.Length - 1; i++) {
            if (trackedObject.x > allWaypoints[i].position.x && trackedObject.x < allWaypoints[i + 1].position.x) {
                pointFound = true;
                // get start point
                Vector3 startPoint = allWaypoints[i].position;
                // distance of x into dolly line (0-1)
                float distance = 1.0f - (allWaypoints[i].endPosition.x - trackedObject.x) / (allWaypoints[i].endPosition.x - allWaypoints[i].position.x);
                // multiply vector by distance
                reVector = Dc2dUtils.CalculateCubicBezierPoint(distance, allWaypoints[i].position, allWaypoints[i].position + allWaypoints[i].tanOne, allWaypoints[i].endPosition + allWaypoints[i].tanTwo, allWaypoints[i].endPosition);
            }
        }
        // if lock to track
        if (lockOnTrack) {
            if (trackedObject.x < allWaypoints[0].position.x) {
                reVector = allWaypoints[0].position;
                pointFound = true;
            } else if (trackedObject.x > allWaypoints[allWaypoints.Length - 1].position.x) {
                reVector = allWaypoints[allWaypoints.Length - 1].position;
                pointFound = true;
            }
        }
        if (!pointFound) {
            reVector = trackedObject;
        }
        return reVector;
    }

    public Vector3 getPositionOnTrack(Vector3 trackedObject) {
        if( mode == DollyMode.TrackTimeProOnly && proOnlyMessage == false) {
            Debug.Log("Track Time is a pro only feature. Defaulting to tracked Object");
            mode = DollyMode.TrackObject;
            proOnlyMessage = true;
        }
        // pro only time tracking
        return getTrackedObjectBasedPosition(trackedObject);
    }

    public void straightenWaypoint(Dc2dWaypoint first, Dc2dWaypoint second) {
        // pro only
    }

    public void smoothWaypoint(Dc2dWaypoint first, Dc2dWaypoint second) {
       // pro only
    }

    public void evenWaypointTimes() {
       // pro only
    }

    public float FastArcLength(Dc2dWaypoint wp) {
        float arcLength = 0.0f;
        Dc2dUtils.ArcLengthUtil(wp.position, wp.position+wp.tanOne, wp.endPosition+wp.tanTwo, wp.endPosition, 5, ref arcLength);
        return arcLength;
    }
}
