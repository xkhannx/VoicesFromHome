using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncer : MonoBehaviour
{
    public float bias;
    public float timeStep;
    public float timeToBeat;
    public float restSmoothTime;
    [Range(0, 127)]
    public float targetFreq;

    float m_previousAudioValue;
    float m_audioValue;
    float m_timer;

    protected bool m_isBeat;

    AudioSpectrum spectrum;
    private void Awake()
    {
        spectrum = FindObjectOfType<AudioSpectrum>();
    }
    void Update()
    {
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        if (spectrum == null) return;

        m_previousAudioValue = m_audioValue;
        
        m_audioValue = spectrum.SpectrumValAt(targetFreq);
        Debug.Log(m_audioValue);

        if (m_previousAudioValue > bias && m_audioValue <= bias)
        {
            if (m_timer > timeStep)
            {
                OnBeat();
            }
        }

        if (m_previousAudioValue <= bias && m_audioValue > bias)
        {
            if (m_timer > timeStep)
            {
                OnBeat();
            }
        }

        m_timer += Time.deltaTime;
    }

    public virtual void OnBeat()
    {
        Debug.Log("beat");
        m_timer = 0;
        m_isBeat = true;
    }
}
