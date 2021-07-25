using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dc2dUtils
{
    // get a rought estimate of the length of a cubic bezier curve
    public static void ArcLengthUtil(Vector3 A, Vector3 B, Vector3 C, Vector3 D, uint subdiv, ref float L) {
        if (subdiv > 0) {
            Vector3 a = A + (B - A) * 0.5f;
            Vector3 b = B + (C - B) * 0.5f;
            Vector3 c = C + (D - C) * 0.5f;
            Vector3 d = a + (b - a) * 0.5f;
            Vector3 e = b + (c - b) * 0.5f;
            Vector3 f = d + (e - d) * 0.5f;

            // left branch
            ArcLengthUtil(A, a, d, f, subdiv - 1, ref L);
            // right branch
            ArcLengthUtil(f, e, c, D, subdiv - 1, ref L);
        } else {
            float controlNetLength = (B - A).magnitude + (C - B).magnitude + (D - C).magnitude;
            float chordLength = (D - A).magnitude;
            L += (chordLength + controlNetLength) / 2.0f;
        }
    }

    // get a point on a bezier curve at t
    public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }
}
