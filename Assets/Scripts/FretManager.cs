using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FretManager : MonoBehaviour
{
    [SerializeField] CurveCreator curve;

    [Header("Frets and notes")]
    [SerializeField] Transform fretPrefab;
    [SerializeField] Transform notePrefab;
    [SerializeField] Transform runePrefab;
    public void SpawnFrets()
    {
        for (int i = 0; i < curve.song.frets.Count; i++)
        {
            Transform newFret = Instantiate(fretPrefab, transform);
            Quaternion rot;
            newFret.position = curve.PointFromXPos(curve.song.frets[i].xPos, out rot);
            newFret.rotation = rot;
            newFret.position += newFret.rotation * Vector3.up * 0.03f;

            curve.song.frets[i].trans = newFret;
            curve.song.frets[i].noteKilled = new bool[3];

            if (!curve.song.frets[i].mainFret)
            {
                newFret.GetChild(0).gameObject.SetActive(false);
            }

            for (int j = 0; j < 3; j++)
            {
                if (curve.song.frets[i].notePresent[j])
                {
                    Transform newNote = Instantiate(notePrefab, newFret);
                    newNote.localPosition = Vector3.right * (j - 1) * 1.5f;
                    curve.song.frets[i].noteTransforms[j] = newNote;
                    if (!curve.song.frets[i].mainFret)
                    {
                        newFret.GetChild(0).gameObject.SetActive(true);
                        newFret.GetChild(0).position = newNote.position;
                        newFret.GetChild(0).localScale = 0.3f * Vector3.one;
                    }
                }
            }
        }
    }

    private void Start()
    {

        SpawnFrets();
    }
}
