using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Song", menuName = "Song Data", order = 1)]
public class SongData : ScriptableObject
{
    public bool saved;
    public AudioClip clip;
    public float songLen { get {return clip.length;} }
    public float baseBPM;

    public int numBeats;
    public List<FretData> frets;
}
 