using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Curve 
{
    [SerializeField, HideInInspector] List<Vector3> curvePoints;
    [SerializeField, HideInInspector] public List<Quaternion> anchorRots;
    public Curve(Vector3 center, float startLen)
    {
        curvePoints = new List<Vector3>
        {
            center,
            center + Vector3.forward * startLen * 0.25f,
            center + Vector3.forward * startLen * 0.75f,
            center + Vector3.forward * startLen
        };
        anchorRots = new List<Quaternion>
        {
            Quaternion.identity,
            Quaternion.identity
        };
    }

    public Vector3 this[int i]
    {
        get
        {
            return curvePoints[i];
        }
    }
    
    public int NumPoints
    {
        get
        {
            return curvePoints.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return curvePoints.Count / 3;
        }
    }

    public void AddSegment(float length, float pitch, float yaw)
    {
        // first control
        Vector3 control_1 = (curvePoints[curvePoints.Count - 1] - curvePoints[curvePoints.Count - 2]).normalized * 0.25f * length;

        // anchor point
        Quaternion newRot = anchorRots[anchorRots.Count - 1];
        Vector3 localUp = newRot * Vector3.up;
        newRot *= Quaternion.AngleAxis(yaw, localUp);

        Vector3 localRight = newRot * Vector3.right;
        newRot *= Quaternion.AngleAxis(pitch, localRight);

        Vector3 relativeAnchor = newRot * Vector3.forward * length;

        // second control
        Vector3 control_2 = relativeAnchor - 2 * control_1;

        curvePoints.Add(curvePoints[curvePoints.Count - 1] + control_1);
        curvePoints.Add(curvePoints[curvePoints.Count - 2] + relativeAnchor - control_2 / 2);
        curvePoints.Add(curvePoints[curvePoints.Count - 3] + relativeAnchor);

        newRot = Quaternion.FromToRotation(Vector3.forward, (curvePoints[curvePoints.Count - 1] - curvePoints[curvePoints.Count - 2]).normalized);
        anchorRots.Add(newRot);
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] { curvePoints[i * 3], curvePoints[i * 3 + 1], curvePoints[i * 3 + 2], curvePoints[LoopIndex(i * 3 + 3)] };
    }

    public Vector4[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector4> evenPoints = new List<Vector4>();

        Vector3 previousPoint = curvePoints[0];
        evenPoints.Add(curvePoints[0]);

        float dstSinceLastEvenPoint = 0;

        for (int segmentInd = 0; segmentInd < NumSegments; segmentInd++)
        {
            Vector3[] p = GetPointsInSegment(segmentInd);
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            
            float t = 0;
            while (t <= 1)
            {
                t += 1 / (float) divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);

                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;

                    Vector4 newPoint = new Vector4(newEvenPoint.x, newEvenPoint.y, newEvenPoint.z, segmentInd);
                    evenPoints.Add(newPoint);

                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenPoint; 
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenPoints.ToArray();
    }

    public float[] CumEvenPoints(Vector4[] evenPoints)
    {
        float[] pointCumLen = new float[evenPoints.Length];
        pointCumLen[0] = 0;
        for (int i = 1; i < evenPoints.Length; i++)
        {
            pointCumLen[i] = pointCumLen[i - 1] + Vector3.Distance(evenPoints[i - 1], evenPoints[i]);
        }

        return pointCumLen;
    }

    public float SegmentLength(int segmentInd, float spacing, float resolution = 1)
    {
        float totalLen = 0;

        List<Vector3> evenPoints = new List<Vector3>();

        Vector3[] p = GetPointsInSegment(segmentInd);
        Vector3 previousPoint = p[0];
        evenPoints.Add(p[0]);

        float dstSinceLastEvenPoint = 0;
        float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
        float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2;
        int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);

        float t = 0;
        while (t <= 1)
        {
            t += 1 / (float)divisions;
            Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);

            dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

            while (dstSinceLastEvenPoint >= spacing)
            {
                float overshootDst = dstSinceLastEvenPoint - spacing;
                Vector3 newEvenPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                evenPoints.Add(newEvenPoint);
                dstSinceLastEvenPoint = overshootDst;
                previousPoint = newEvenPoint;
            }

            previousPoint = pointOnCurve;
        }
        if (t - 1 / (float)divisions < 1)
            evenPoints.Add(Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], 1));

        for (int i = 1; i < evenPoints.Count; i++)
        {
            totalLen += Vector3.Distance(evenPoints[i], evenPoints[i - 1]);
        }

        return totalLen;
    }

    int LoopIndex(int i)
    {
        return (i + curvePoints.Count) % curvePoints.Count;
    }
}
