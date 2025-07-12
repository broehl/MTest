// Very simple mouth open/close lipsync based on amplitude

// Based on code from mdotstrange/AutoLipSync on github

// Modified to use blendshapes instead of a hinged jaw by Bernie Roehl, November 2024

using UnityEngine;
using UnityEngine.Events;

public class MouthSync : MonoBehaviour
{

    public AudioSource audioSource;
    [Range(1, 50)]
    public float sensitivity;

    [Header("Fires an event when talking starts")]
    public UnityEvent FireOnStartedTalking;

    public int mouthOpenIndex;
    public SkinnedMeshRenderer[] skms;
    public float boost = 1;

    private float lastLoud;
    private float smoothedLoudness;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (audioSource == null)
        {
            lastLoud = 0f;
            return;
        }

        if (audioSource.isPlaying)
        {
            ProcessAudio();
        }
        else
        {
            lastLoud = 0f;
        }
    }

    private void ProcessAudio()
    {
        float loud = GetAveragedVolume() * sensitivity;

        if (loud >= 0.91f && lastLoud <= 0.9f)
        {
            FireOnStartedTalking.Invoke();
        }

        smoothedLoudness = lowPassFilter(loud, ref smoothedLoudness, 0.8f, false);
        for (int i = 0; i < skms.Length; ++i) skms[i].SetBlendShapeWeight(mouthOpenIndex, loud * boost);
      }

    private float GetAveragedVolume()
    {
        float[] data = new float[256];
        float a = 0;
        audioSource.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    private float lowPassFilter(float targetValue, ref float intermediateValueBuf, float factor, bool init)
    {

        float intermediateValue;

        //intermediateValue needs to be initialized at the first usage.
        if (init)
        {
            intermediateValueBuf = targetValue;
        }

        intermediateValue = (targetValue * factor) + (intermediateValueBuf * (1.0f - factor));


        intermediateValueBuf = intermediateValue;

        return intermediateValue;
    }
}
