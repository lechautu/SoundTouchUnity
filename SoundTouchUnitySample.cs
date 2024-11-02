using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace TL.SoundTouch.Unity
{
    public class SoundTouchUnitySample : MonoBehaviour
    {
        int outputSampleRate;

        void Awake()
        {
            PrintVersion();
            outputSampleRate = AudioSettings.outputSampleRate;       
        }

        void PrintVersion()
        {
            Debug.Log("SoundTouch version: " + SoundTouch.Version);
        }

        void Process(float[] data, int channels, int sampleRate, out float[] output)
        {
            SoundTouch st = new()
            {
                Channels = (uint)channels,
                SampleRate = (uint)sampleRate
            };
            st.TempoChange = 100;
            int numSamples = data.Length / channels;
            float[] buffer = new float[1024];
            float[] outputBuffer = new float[(int)(0.5f * numSamples * channels)];
            for (int i = 0; i < data.Length; i += buffer.Length)
            {
                int length = Math.Min(buffer.Length, data.Length - i);
                Array.Copy(data, i, buffer, 0, length);
                st.PutSamples(buffer, (uint)(length / channels));
                st.Flush();
                int receivedSamples = st.ReceiveSamples(outputBuffer, (uint)(outputBuffer.Length / channels));
            }
            Debug.Log("Available: " + receivedSamples);
            if (receivedSamples > 0)
            {
                Array.Copy(buffer, 0, data, 0, receivedSamples * channels);
            }
            st.Dispose();
        }
    }
}