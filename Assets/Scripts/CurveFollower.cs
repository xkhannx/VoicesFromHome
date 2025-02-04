using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveFollower : MonoBehaviour
{
    public CurveCreator curve;
    public AudioSource music;
    List<FretData> frets;
    float musicTime = 0;
    void Update()
    {
        if (music.isPlaying)
        {
            musicTime += Time.deltaTime;

            if (musicTime > beatCumTime[curFretInd + 1])
            {
                curFretInd++;
            }

            float xPos = LengthFromMusicTime();

            Quaternion rot;
            transform.position = curve.PointFromXPos(xPos, out rot); 
            transform.rotation = rot;
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartSong();
            }
        }
    }

    private float LengthFromMusicTime()
    {
        float curBeatPercent = (musicTime - beatCumTime[curFretInd]) / frets[curFretInd].beatTime;

        return frets[curFretInd].xPos + frets[curFretInd].beatLen * curBeatPercent;
    }

    

    int curFretInd = 0;
    float[] beatCumTime;
    void StartSong()
    {
        frets = curve.song.frets;
        
        beatCumTime = new float[curve.song.numBeats + 1];

        beatCumTime[0] = 0;
        for (int i = 0; i < curve.song.numBeats; i++)
        {
            beatCumTime[i + 1] = beatCumTime[i] + frets[i].beatTime;
        }
        music.Play();
    }
}
