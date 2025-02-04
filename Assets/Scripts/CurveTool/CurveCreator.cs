using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveCreator : MonoBehaviour
{
    [HideInInspector] public Curve curve;

    [Header("Curve params")]
    public SongData song;
    public float maxYaw;
    public float maxPitch;
    public int segmentBeats = 8;
    public AnimationCurve rotsBlendCurve;

    [Header("Road mesh params")]
    public RoadMeshCreator road;
    public float roadWidth = 4.5f;
    [Range(0.05f, 5)] public float vertexSpacing = 1;

    [Header("GUI Handle params")]
    public int drawDistance = 100;
    public int maxDrawPoints = 500;

    [SerializeField, HideInInspector] public List<Quaternion> rots;
    [SerializeField, HideInInspector] public Vector4[] points;
    [SerializeField, HideInInspector] float[] pointCumLen;
    void Start()
    {
    }

    public void CreateRoad()
    {
        CreateCurve();

        UpdateRoad();
    }

    [SerializeField, HideInInspector] public float songFretLen;
    [SerializeField, HideInInspector] public float roadMeshLen;
    private void CreateEvenPoints()
    {
        points = curve.CalculateEvenlySpacedPoints(vertexSpacing);
        pointCumLen = curve.CumEvenPoints(points);

        songFretLen = (song.frets[song.frets.Count - 1].xPos + song.frets[song.frets.Count - 1].beatLen);
        roadMeshLen = pointCumLen[pointCumLen.Length - 1];
    }

    void CreateCurve()
    {
        int startInd = 16;
        float startLen = 0;
        for (int i = 0; i < song.frets.Count; i++)
        {
            if (song.frets[i].label == "Start")
            {
                startInd = i;
                break;
            }
            startLen += song.frets[i].beatLen;
        }

        float fretsLen = startLen;
        float prevFretsLen = startLen;

        curve = new Curve(transform.position, startLen);
        float prevSegOvershoot = curve.SegmentLength(0, vertexSpacing) - startLen;

        int mainBeatCount = 0;
        for (int i = startInd; i < song.frets.Count; i++)
        {
            fretsLen += song.frets[i].beatLen;
            if (song.frets[i].mainFret)
            {
                mainBeatCount++;
                if (mainBeatCount == segmentBeats)
                {
                    float newSegLen = fretsLen - prevFretsLen - prevSegOvershoot;
                    float nextPitch = Random.Range(0, maxPitch);
                    float yaw = maxYaw * Random.Range(-1f, 1f);

                    curve.AddSegment(newSegLen, -nextPitch, yaw);

                    float segLen = curve.SegmentLength(curve.NumSegments - 1, vertexSpacing);

                    prevSegOvershoot = segLen - newSegLen;

                    prevFretsLen = fretsLen;
                    mainBeatCount = 0;
                }
            }
        }
        curve.AddSegment(fretsLen - prevFretsLen - prevSegOvershoot, 0, 0);
    }

    
    public void UpdateRoad()
    {
        CreateEvenPoints();

        rots = new List<Quaternion>();
        rots.Add(curve.anchorRots[0]);
        float curLen = 0;
        int prevSeg = 0;
        float segLen = curve.SegmentLength(0, vertexSpacing);
        for (int i = 1; i < points.Length; i++)
        {
            int curSeg = Mathf.RoundToInt(points[i].w);

            if (prevSeg != curSeg)
            {
                curLen -= segLen;
                prevSeg = curSeg;
                segLen = curve.SegmentLength(curSeg, vertexSpacing);
            }

            curLen += Vector3.Distance(points[i], points[i - 1]);

            float t = Mathf.Clamp01(curLen / segLen);

            Quaternion evenRot = Quaternion.Slerp(curve.anchorRots[curSeg], curve.anchorRots[curSeg + 1], rotsBlendCurve.Evaluate(t));
            rots.Add(evenRot);
        }

        road.UpdateRoadMesh(points, rots.ToArray(), vertexSpacing, roadWidth);
    }

    public Vector3 PointFromXPos(float xPos, out Quaternion rot)
    {
        for (int i = 1; i < points.Length; i++)
        {
            if (xPos < pointCumLen[i])
            {
                rot = Quaternion.Slerp(rots[i - 1], rots[i], (xPos - pointCumLen[i - 1]) / (pointCumLen[i] - pointCumLen[i - 1]));
                return Vector3.Lerp(points[i - 1], points[i], (xPos - pointCumLen[i - 1]) / (pointCumLen[i] - pointCumLen[i - 1]));
            }
        }

        rot = rots[points.Length - 1];
        return points[points.Length - 1];
    }

}
