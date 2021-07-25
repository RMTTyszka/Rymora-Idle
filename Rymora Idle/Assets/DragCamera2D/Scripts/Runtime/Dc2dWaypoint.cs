using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dc2dWaypoint 
{
    // properties
    [Tooltip("Waypoint position")]
    public Vector3 position;
    [Tooltip("Exiting Curcve tangent to next waypoint")]
    public Vector3 tanOne = new Vector3(0.3333f, 0, 0);
    [Tooltip("Entering Curve tangent from previous waypoint")]
    public Vector3 tanTwo = new Vector3(-0.3333f,0,0);
    [Tooltip("Position of next waypoint")]
    public Vector3 endPosition;
    [Tooltip("Time in seconds for camera to reach next waypoint")]
    public float timeToNextWaypoint = 1f;
    public float arclength = 0f;

    public Vector3 getPostSpawn() {
        Vector3 post = position + new Vector3(1, 0, 0);
        return post;
    }

    public Vector3 getPreSpawn() {
        Vector3 pre = position + new Vector3(-1, 0, 0);
        return pre;
    }

    public float getArcLength() {
        return arclength;
    }
}
