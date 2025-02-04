using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnSongRoad))]
public class SongParamsEditor : Editor
{
    SpawnSongRoad songEditor;
    private void OnEnable()
    {
        songEditor = (SpawnSongRoad)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Spawn frets"))
        {
            Undo.RecordObject(songEditor, "Spawn frets");

            songEditor.SpawnFrets();
            SceneView.RepaintAll();
        }
    }


}
