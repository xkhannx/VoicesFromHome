using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurveCreator))]
public class CurveEditor : Editor
{
    CurveCreator creator;
    Curve curve;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(creator, "Create new");
            creator.CreateRoad();
            curve = creator.curve;
            SceneView.RepaintAll();
        }
        GUILayout.Label("Curve segments: " + curve.NumSegments);
        GUILayout.Label("Curve points: " + curve.NumPoints);
        GUILayout.Label("Even points: " + creator.points.Length);
        GUILayout.Label("Song beat length: " + creator.songFretLen);
        GUILayout.Label("Road mesh length: " + creator.roadMeshLen);
    }

    private void OnSceneGUI()
    {
        if (creator == null || creator.curve == null || curve.anchorRots == null)
        {
            creator = (CurveCreator)target;
            if (creator.curve == null)
            {
                creator.CreateRoad();
            }
            curve = creator.curve;
        }
        Draw();
    }

    void Draw()
    {
        for (int i = 0; i < curve.NumSegments; i++)
        {
            Vector3[] points = curve.GetPointsInSegment(i);

            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }
        Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
        for (int i = 0; i < curve.NumPoints; i++)
        {
            Handles.color = i % 3 == 0 ? Color.red : Color.white;
            Handles.SphereHandleCap(0, curve[i], Quaternion.identity, 0.1f, EventType.Repaint);
            if (i % 3 == 0)
            {
                if (curve.anchorRots != null && i < curve.anchorRots.Count * 3)
                {
                    if (Vector3.Distance(camPos, curve[i]) < creator.drawDistance)
                    {
                        Handles.color = Color.green;
                        Handles.DrawLine(curve[i], curve[i] + curve.anchorRots[i / 3] * Vector3.up * 5);
                        Handles.color = Color.red;
                        Handles.DrawLine(curve[i], curve[i] + curve.anchorRots[i / 3] * Vector3.right * 3);
                        Handles.color = Color.blue;
                        Handles.DrawLine(curve[i], curve[i] + curve.anchorRots[i / 3] * Vector3.forward * 3);
                        Handles.color = Color.yellow;
                        Quaternion newRot = Handles.Disc(curve.anchorRots[i / 3], curve[i], curve.anchorRots[i / 3] * Vector3.forward, 3, false, 0);

                        if (curve.anchorRots[i / 3] != newRot)
                        {
                            Undo.RecordObject(creator, "Rotate normal");
                            curve.anchorRots[i / 3] = newRot;

                            creator.UpdateRoad();
                        }
                    }
                }
            }
        }

        if (creator.points == null || creator.points.Length == 0) return;

        for (int i = 0; i < creator.points.Length; i++)
        {
            if (Vector3.Distance(camPos, creator.points[i]) < creator.drawDistance)
            {
                if (creator.points.Length < creator.maxDrawPoints || i % (creator.points.Length / creator.maxDrawPoints) == 0)
                {
                    Handles.color = Color.black;
                    var fmh_95_63_638740368365452062 = Quaternion.identity; Handles.FreeMoveHandle(creator.points[i], 0.5f, Vector3.zero, Handles.SphereHandleCap);
                    Vector3 pointPos = creator.points[i];
                    Handles.color = Color.cyan;
                    if (creator.rots != null && creator.rots.Count > i)
                        Handles.DrawLine(pointPos, pointPos + creator.rots[i] * Vector3.up * 2);
                }
            }
        }
    }

    private void OnEnable()
    {
        creator = (CurveCreator)target;
        if (creator.curve == null)
        {
            creator.CreateRoad();
        }
        curve = creator.curve;
    }
}
