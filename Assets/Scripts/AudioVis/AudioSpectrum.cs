using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpectrum : MonoBehaviour
{
    float[] spectrumValue;

    public float SpectrumValAt(float targetFreq)
    {
        if (spectrumValue != null && spectrumValue.Length > 0)
            return 1000 * spectrumValue[Mathf.RoundToInt(Mathf.Clamp(targetFreq, 0, spectrumValue.Length - 1))];
        else
            return 0;
    }
    private float[] m_audioSpectrum;
    void Start()
    {
        m_audioSpectrum = new float[128];
    }

    void Update()
    {
        AudioListener.GetSpectrumData(m_audioSpectrum, 0, FFTWindow.Hamming);

        if (m_audioSpectrum != null && m_audioSpectrum.Length > 0)
        {
            spectrumValue = m_audioSpectrum;
        }
    }
}
