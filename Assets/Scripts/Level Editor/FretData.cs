using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FretData 
{
    public Transform trans;
    public float bpm;
    public float xPos;
    public bool mainFret;
    public float speed;

    public bool[] notePresent;
    public bool[] noteKilled;
    public Transform[] noteTransforms;

    public string label;

    public float beatTime { get { return 60 / bpm; } }
    public float beatLen { get { return 60 / bpm * speed; } }
}
