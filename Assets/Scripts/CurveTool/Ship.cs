using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] CurveCreator curve;
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource tick;
    [SerializeField] MeshRenderer[] fretButton;
    [SerializeField] [ColorUsage(false, true)] Color fretButtonColorWrong;
    [SerializeField] [ColorUsage(false, true)] Color fretButtonColorON;
    [SerializeField] [ColorUsage(false, true)] Color fretButtonColorOFF;
    [SerializeField] Transform traceNotePrefab;
    [SerializeField] FretButton[] fretTouchButtons;
    public void StartSong()
    {
        fretButton[0].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", fretButtonColorOFF);
        fretButton[1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", fretButtonColorOFF);
        fretButton[2].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", fretButtonColorOFF);
        music.clip = curve.song.clip;
        music.Play();
        fretButton[0].material.SetFloat("_NoiseHeightScale", -0.1f);
        fretButton[1].material.SetFloat("_NoiseHeightScale", -0.1f);
        fretButton[2].material.SetFloat("_NoiseHeightScale", -0.1f);
    }

    public bool autoPlay;
    float fretsCumTime = 0;
    int curFretInd = 0;
    int prevFretInd = 0;

    Vector3 velocity = Vector3.zero;

    float musicTime = 0;
    void Update()
    {
        if (music.isPlaying && curFretInd < curve.song.frets.Count - 1)
        {
            musicTime += Time.deltaTime;
            if (musicTime >= fretsCumTime + curve.song.frets[curFretInd].beatTime)
            {
                fretsCumTime += curve.song.frets[curFretInd].beatTime;
                curFretInd++;
            }

            if (curFretInd != prevFretInd)
            {
                if (autoPlay)
                {
                    if (curve.song.frets[curFretInd].notePresent[0] || curve.song.frets[curFretInd].notePresent[1] || curve.song.frets[curFretInd].notePresent[2])
                    {
                        tick.Play();

                        StartCoroutine(KillNotes(curve.song.frets[curFretInd]));
                    }
                }
                prevFretInd = curFretInd;
            }

            if (curFretInd < curve.song.frets.Count - 1)
            {
                //Quaternion rot;
                //Vector3 targetPos = curve.curve.PointFromXPos(LengthFromMusicTime(musicTime), curve.points, curve.rots, out rot);
                //transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 0.1f);
                //transform.rotation = rot;
            }
        }
        else
        if (Input.GetMouseButtonDown(0))
        {
            StartSong();
        }

        if (!autoPlay)
        {
            for (int i = 0; i < 3; i++)
            {
                if (fretTouchButtons[i].pressed)
                {
                    fretTouchButtons[i].pressed = false;
                    if (curve.song.frets[curFretInd].notePresent[i] && Vector3.Distance(fretButton[i].transform.position, curve.song.frets[curFretInd].noteTransforms[i].position) <= hitTolerance
                        && !curve.song.frets[curFretInd].noteKilled[i])
                    {
                        curve.song.frets[curFretInd].noteKilled[i] = true;
                        killedNotes.Add(curve.song.frets[curFretInd]);
                        tick.Play();

                        StartCoroutine(KillNotes(curve.song.frets[curFretInd]));
                        StartCoroutine(FlashButton(i, fretButtonColorON));
                    } else 
                    if (curve.song.frets[curFretInd + 1].notePresent[i] && Vector3.Distance(fretButton[i].transform.position, curve.song.frets[curFretInd + 1].noteTransforms[i].position) <= hitTolerance
                        && !curve.song.frets[curFretInd + 1].noteKilled[i])
                    {
                        curve.song.frets[curFretInd + 1].noteKilled[i] = true;
                        killedNotes.Add(curve.song.frets[curFretInd + 1]);
                        tick.Play();

                        StartCoroutine(KillNotes(curve.song.frets[curFretInd + 1]));
                        StartCoroutine(FlashButton(i, fretButtonColorON));
                    } else
                    {
                        StartCoroutine(FlashButton(i, fretButtonColorWrong));
                    }
                }
            }
        }
    }
    public float delay = 0.5f;
    public float hitTolerance = 0.5f;
    IEnumerator KillNotes(FretData fret)
    {
        List<Transform> notes = new List<Transform>();
        for (int i = 0; i < fret.noteTransforms.Length; i++)
        {
            if (fret.notePresent[i])
            {
                fretButton[i].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", fretButtonColorON);
                fretButton[i].material.SetFloat("_NoiseHeightScale", -1f);
                fretButton[i].transform.GetComponentInChildren<ParticleSystem>().Play();

                fret.noteTransforms[i].parent = transform;
                notes.Add(fret.noteTransforms[i]);
            }
        }
        float t = 0;
        while (t <= 0.2f)
        {
            t += Time.deltaTime;
            for (int i = 0; i < fret.noteTransforms.Length; i++)
            {
                if (fret.notePresent[i])
                {
                    fretButton[i].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.Lerp(fretButtonColorON, fretButtonColorOFF, t / 0.2f));
                    fretButton[i].material.SetFloat("_NoiseHeightScale", Mathf.Lerp(-1f, -0.1f, t / 0.2f));
                    fret.noteTransforms[i].parent = transform;
                    fret.noteTransforms[i].localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t / 0.2f);
                }
            }
            yield return null;
        }

    }
    IEnumerator FlashButton(int buttonInd, Color flashColor)
    {
        fretButton[buttonInd].GetComponent<MeshRenderer>().material.SetColor("_Color", flashColor);
        fretButton[buttonInd].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", flashColor);

        float t = 0;
        while (t <= 0.2f)
        {
            t += Time.deltaTime;
            fretButton[buttonInd].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(flashColor, fretButtonColorOFF, t / 0.2f));
            fretButton[buttonInd].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.Lerp(flashColor, fretButtonColorOFF, t / 0.2f));
            yield return null;
        }

    }
    float LengthFromMusicTime(float musicTime)
    {
        float totalLen = 0;
        float musicT = musicTime;
        for (int i = 0; i < curve.song.frets.Count - 1; i++)
        {
            if (musicT <= curve.song.frets[i].beatTime)
            {
                return totalLen + Mathf.Lerp(0, curve.song.frets[i].beatLen, musicT / curve.song.frets[i].beatTime);
            }

            totalLen += curve.song.frets[i].beatLen;
            musicT -= curve.song.frets[i].beatTime;
            
        }
        return totalLen;
    }

    List<FretData> killedNotes = new();
    private void OnDisable()
    {
        if (killedNotes == null) return;
        foreach (var item in killedNotes)
        {
            for (int i = 0; i < 3; i++)
            {
                item.noteKilled[i] = false;
            }
        }
    }
}
