using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpectrumVisualiser : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject[] visualizerObjects;
    public int sampleSize = 512;
    public float heightMultiplier = 200.0f;
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    private float[] spectrumData;

    void Start()
    {
        spectrumData = new float[sampleSize];    
    }

    // Update is called once per frame
    void Update()
    {
        AnalyseAudio();
        UpdateVisuals();
    }

    void AnalyseAudio()
    {
        audioSource.GetSpectrumData(spectrumData, 0, fftWindow);
    }

    void UpdateVisuals()
    {
        int spectrumSegmentSize = sampleSize / visualizerObjects.Length;

        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            float average = 0.0f;
            int start = i * spectrumSegmentSize;
            int end = start + spectrumSegmentSize;

            for (int j = start; j < end; j++)
            {
                average += spectrumData[j];
            }
            average /= spectrumSegmentSize;

            Vector3 newScale = visualizerObjects[i].transform.localScale;
            newScale.y = 1 + average * heightMultiplier;
            visualizerObjects[i].transform.localScale = newScale;
        }
    }
}
