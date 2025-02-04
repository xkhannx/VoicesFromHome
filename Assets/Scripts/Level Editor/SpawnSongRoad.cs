using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnSongRoad : MonoBehaviour
{
    [SerializeField] SongData song;
    [Header("Prefabs")]
    [SerializeField] Transform fretPrefab;
    [SerializeField] Transform subfretPrefab;
    [SerializeField] Transform notePrefab;
    [SerializeField] Transform runePrefab;
    [Header("References in scene")]
    [SerializeField] Transform fretParent;
    [SerializeField] MeshRenderer finishMat;
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource tick;
    public float moveSpeed = 5;
    [Header("UI stuff")]
    [SerializeField] Text bpmText;
    [SerializeField] Text speedText;
    [SerializeField] Text timeText;

    public List<FretData> frets;
    Transform cam;
    void Start()
    {
        cam = Camera.main.transform;
        
        Color newColor = Color.green;
        newColor *= 0.5f;
        finishMat.sharedMaterial.SetColor("_BaseColor", newColor);
        
        SpawnFrets();
        FindObjectOfType<LabelsManager>().SpawnLabels();
    }

    float beatTime;
    float beatLen;
    public bool resetSong;
    public void SpawnFrets()
    {
        if (song.frets != null && !resetSong)
        {
            frets = song.frets;
            bpmText.text = frets[0].bpm.ToString();

            for (int i = 0; i < song.numBeats; i++)
            {
                FretData newFret = frets[i];
                if (newFret.trans != null) Destroy(newFret.trans.gameObject);
                Transform fretType = newFret.mainFret ? fretPrefab : subfretPrefab;
                newFret.trans = Instantiate(fretType, fretParent);
                newFret.trans.position = transform.position + Vector3.right * (newFret.xPos);
            }

            LoadNotes();
        }
        else
        {
            frets = new List<FretData>();

            bpmText.text = song.baseBPM.ToString();

            beatTime = 60 / song.baseBPM;
            beatLen = beatTime * moveSpeed;
            song.numBeats = Mathf.RoundToInt(music.clip.length / beatTime);
            
            for (int i = 0; i < song.numBeats; i++)
            {
                FretData newFret = new FretData();
                newFret.trans = Instantiate(fretPrefab, fretParent);
                newFret.bpm = song.baseBPM;
                newFret.xPos = i * beatLen;
                newFret.trans.position = transform.position + Vector3.right * (newFret.xPos);
                newFret.mainFret = true;
                newFret.speed = moveSpeed;
                newFret.notePresent = new bool[3];
                newFret.noteTransforms = new Transform[3];
                newFret.label = "";

                frets.Add(newFret);
            }
        }

        song.frets = new List<FretData>(frets);
    }

    bool playing;
    public bool movingTo;
    public bool inMenu;
    public int curFretInd = 0;
    int prevFretInd = 0;
    void Update()
    {
        if (inMenu) return;
        if (curFretInd >= frets.Count - 1) return;
        bpmText.text = "bpm: " + frets[curFretInd].bpm.ToString();
        speedText.text = "speed: " + frets[curFretInd].speed.ToString();
        timeText.text = "t: " + MusicTime().ToString();
        PlayOrStop();
        if (playing)
        {
            timeText.text = "t: " + music.time.ToString();
            if (cam.position.x >= frets[curFretInd + 1].xPos)
            {
                curFretInd++;
            }
            
            if (curFretInd != prevFretInd)
            {
                if (frets[curFretInd].notePresent[0] || frets[curFretInd].notePresent[1] || frets[curFretInd].notePresent[2])
                {
                    tick.Play();
                    StartCoroutine(FlashColor());
                }
                prevFretInd = curFretInd;
            }

            cam.position += Vector3.right * frets[curFretInd].speed * Time.deltaTime;
        }
        else
        { // not playing
            if (!movingTo)
            {
                if (Input.GetKey(KeyCode.A) && curFretInd > 0)
                {
                    curFretInd--;
                    movingTo = true;
                }
                if (Input.GetKey(KeyCode.D) && curFretInd < frets.Count - 1)
                {
                    curFretInd++;
                    movingTo = true;
                }
                if (movingTo)
                {
                    StartCoroutine(MoveToFret());
                    return;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    UpdateBPM(-1);
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    UpdateBPM(1);
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    InsertSubfret();
                    FindObjectOfType<LabelsManager>().SpawnLabels();
                }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    DeleteSubfrets();
                    FindObjectOfType<LabelsManager>().SpawnLabels();
                }
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    UpdateMoveSpeed(-1);
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    UpdateMoveSpeed(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    PlaceNote(0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    PlaceNote(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    PlaceNote(2);
                }
            }
        }
    }


    #region Notes
    private void PlaceNote(int noteInd)
    {
        if (!frets[curFretInd].notePresent[noteInd])
        {
            frets[curFretInd].notePresent[noteInd] = true;
            Transform newNote = Instantiate(notePrefab, frets[curFretInd].trans);
            newNote.position = new Vector3(frets[curFretInd].xPos, 1 - noteInd, -1);
            frets[curFretInd].noteTransforms[noteInd] = newNote;
        }
        else
        {
            frets[curFretInd].notePresent[noteInd] = false;
            Destroy(frets[curFretInd].noteTransforms[noteInd].gameObject);
        }
    }

    private void LoadNotes()
    {
        for (int i = 0; i < frets.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (frets[i].notePresent[j])
                {
                    Transform newNote = Instantiate(notePrefab, frets[i].trans);
                    newNote.position = new Vector3(frets[i].xPos, 1 - j, -1);
                    frets[i].noteTransforms[j] = newNote;
                }
            }
        }
    }
    #endregion

    #region Edit Frets (update bpm, speed, insert and delete subfrets)
    private void DeleteSubfrets()
    {
        while (!frets[curFretInd].mainFret)
            curFretInd--;

        while (!frets[curFretInd + 1].mainFret)
        {
            FretData deadFret = frets[curFretInd + 1];
            frets.RemoveAt(curFretInd + 1);
            Destroy(deadFret.trans.gameObject);
            song.numBeats--;
        }

        beatLen = frets[curFretInd + 1].xPos - frets[curFretInd].xPos;
        beatTime = beatLen / frets[curFretInd].speed;
        frets[curFretInd].bpm = 60 / beatTime;

        movingTo = true;
        StartCoroutine(MoveToFret());
    }

    private void InsertSubfret()
    {
        frets[curFretInd].bpm *= 2;

        FretData newFret = new FretData();
        newFret.trans = Instantiate(subfretPrefab, fretParent);
        newFret.bpm = frets[curFretInd].bpm;
        newFret.speed = frets[curFretInd].speed;
        newFret.notePresent = new bool[3];
        newFret.noteTransforms = new Transform[3];
        newFret.label = "";

        beatTime = 60 / newFret.bpm;
        beatLen = beatTime * frets[curFretInd].speed;

        newFret.xPos = frets[curFretInd].xPos + beatLen;
        newFret.trans.position = Vector3.right * (newFret.xPos);
        
        frets.Insert(curFretInd + 1, newFret);
        song.numBeats++;
    }

    private void UpdateBPM(float bpmChange)
    {
        if (!frets[curFretInd].mainFret || !frets[curFretInd + 1].mainFret) return;

        float curBpm = frets[curFretInd].bpm;
        frets[curFretInd].bpm += bpmChange;

        beatTime = 60 / frets[curFretInd].bpm;
        beatLen = beatTime * frets[curFretInd].speed;

        int i;
        for (i = curFretInd + 1; i < frets.Count; i++)
        {
            frets[i].xPos = frets[i - 1].xPos + beatLen;
            frets[i].trans.position = Vector3.right * frets[i].xPos;

            if (curBpm != frets[i].bpm) break;
            frets[i].bpm = frets[curFretInd].bpm;
        }
        for (int j = i + 1; j < frets.Count; j++)
        {
            beatTime = 60 / frets[j - 1].bpm;
            beatLen = beatTime * frets[j - 1].speed;
            frets[j].xPos = frets[j - 1].xPos + beatLen;
            frets[j].trans.position = Vector3.right * frets[j].xPos;
        }
    }
    private void UpdateMoveSpeed(float speedChange)
    {
        float curSpeed = frets[curFretInd].speed;
        frets[curFretInd].speed += speedChange;

        int i;
        for (i = curFretInd + 1; i < frets.Count; i++)
        {
            beatTime = 60 / frets[i - 1].bpm;
            beatLen = beatTime * frets[i - 1].speed;
            frets[i].xPos = frets[i - 1].xPos + beatLen;
            frets[i].trans.position = Vector3.right * frets[i].xPos;

            if (curSpeed != frets[i].speed) break;
            frets[i].speed = frets[curFretInd].speed;
        }

        for (int j = i + 1; j < frets.Count; j++)
        {
            beatTime = 60 / frets[j - 1].bpm;
            beatLen = beatTime * frets[j - 1].speed;
            frets[j].xPos = frets[j - 1].xPos + beatLen;
            frets[j].trans.position = Vector3.right * frets[j].xPos;
        }
    }
    #endregion
    private void PlayOrStop()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!playing)
            {
                if (music.clip == null) music.clip = song.clip;

                music.time = MusicTime();
                music.Play();

                playing = true;
            }
            else
            {
                movingTo = true;
                StartCoroutine(MoveToFret());

                music.Stop();
                playing = false;
            }
        }
    }

    private float MusicTime()
    {
        float musicTime = 0;
        for (int i = 1; i <= curFretInd; i++)
        {
            beatTime = 60 / frets[i - 1].bpm;
            musicTime += beatTime;
        }

        return musicTime;
    }

    #region Coroutines (move to fret, flash)
    public IEnumerator MoveToFret()
    {
        Vector3 initPos = cam.position;
        Vector3 targetPos = frets[curFretInd].trans.position;
        
        targetPos.z = -10;

        float t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            cam.position = Vector3.Lerp(initPos, targetPos, t / 0.1f);
            yield return null;
        }
        movingTo = false;
    }

    IEnumerator FlashColor()
    {
        Color initCol = finishMat.sharedMaterial.GetColor("_BaseColor");
        Color targetCol = Color.white;
        finishMat.sharedMaterial.SetColor("_BaseColor", targetCol);

        float t = 0;
        while (t < 0.05f)
        {
            t += Time.deltaTime;
            finishMat.sharedMaterial.SetColor("_BaseColor", Color.Lerp(targetCol, initCol, t / 0.05f));
            yield return null;
        }

        movingTo = false;
    }
    #endregion

    //    private void OnDisable()
    //    {
    //        song.frets = new List<FretData>(frets);
    //        UnityEditor.EditorUtility.SetDirty(song);
    //    }
}
