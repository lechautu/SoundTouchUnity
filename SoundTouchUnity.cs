using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TL.SoundTouch.Unity
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundTouchUnity : MonoBehaviour
    {
        const int BLOCK_SIZE = 512;

        SoundTouch soundTouch;
        CircularBuffer inBuffer, outBuffer;
        AudioSource audioSource;
        AudioClip dynamicClip;
        float[] processedBuffer;
        float[] audioInputData;
        int audioReadPos;

        [SerializeField] AudioClip audioClip;
        public AudioClip Clip
        {
            get => audioClip;
            set
            {
                audioClip = value;
                SetupClip();
            }
        }

        float tempo = 1;
        public float Tempo
        {
            get => tempo;
            set
            {
                tempo = value;
                SetupTempo();
            }
        }

        bool isPlaying;
        public bool IsPlaying => isPlaying && audioInputData != null && audioReadPos < audioInputData.Length;

        public float Duration => audioClip != null ? audioClip.length : 0;
        public float Time => audioClip != null && audioInputData != null ? 1.0f * audioReadPos / audioInputData.Length * audioClip.length : 0;

        public void Play()
        {
            if (audioClip == null)
            {
                Debug.LogWarning("clip is not set");
                return;
            }
            isPlaying = true;
            audioSource.Play();
        }

        public void Pause()
        {
            isPlaying = false;
            audioSource.Pause();
        }

        public void Stop()
        {
            isPlaying = false;
            audioSource.Stop();
            ResetComponent();
        }

        public void Seek(float seconds)
        {
            soundTouch.Clear();
            inBuffer.Clear();
            outBuffer.Clear();
            seconds = Mathf.Clamp(seconds, 0, audioClip.length);
            audioReadPos = (int)(seconds / audioClip.length * audioInputData.Length);
        }

        #region Private methods
        void Awake()
        {
            PrintVersion();

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;

            inBuffer = new CircularBuffer();
            outBuffer = new CircularBuffer();
            processedBuffer = new float[8192];
            soundTouch = new();
        }

        void Start()
        {
            SetupClip();
        }

        void OnDestroy()
        {
            soundTouch.Dispose();
            inBuffer.Dispose();
            outBuffer.Dispose();
        }

        void SetupClip()
        {
            isPlaying = false;
            audioSource.Stop();
            if (audioClip == null)
            {
                return;
            }

            audioInputData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioInputData, 0);
            if (dynamicClip == null || dynamicClip.frequency != audioClip.frequency || dynamicClip.channels != audioClip.channels)
            {
                int channels = audioClip.channels;
                int freq = audioClip.frequency;
                soundTouch.Channels = (uint)channels;
                soundTouch.SampleRate = (uint)freq;
                audioSource.clip = CreateDynamicClip(channels, freq, ref dynamicClip);
                audioSource.Play();
            }

            ResetComponent();
        }

        AudioClip CreateDynamicClip(int channels, int frequency, ref AudioClip dynamicClip)
        {
            if (dynamicClip != null)
            {
                Destroy(dynamicClip);
            }
            dynamicClip = AudioClip.Create(string.Empty, 1024, channels, frequency, true, PCMReaderCallback);
            return dynamicClip;
        }

        void SetupTempo()
        {
            if (soundTouch == null)
            {
                return;
            }
            soundTouch.Tempo = tempo;
        }

        void PCMReaderCallback(float[] data)
        {
            // if (!isPlaying)
            // {
            //     for (int i = 0; i < data.Length; i++)
            //     {
            //         data[i] = 0;
            //     }
            //     return;
            // }
            int needData = data.Length;
            while (outBuffer.Available < needData && audioReadPos < audioInputData.Length)
            {
                Process(needData);
            }
            for (int i = 0; i < data.Length; i++)
            {
                if (outBuffer.Available > 0)
                {
                    outBuffer.Read(data, i, 1);
                }
                else
                {
                    data[i] = 0;
                }
            }
        }

        void Process(int length)
        {
            if (inBuffer == null || outBuffer == null)
            {
                return;
            }

            int readLength = Mathf.Min(length, audioInputData.Length - audioReadPos);
            inBuffer.Write(audioInputData, audioReadPos, readLength);
            audioReadPos += readLength;

            while (inBuffer.Available >= BLOCK_SIZE)
            {
                var block = new float[BLOCK_SIZE];
                inBuffer.Read(block, 0, block.Length);
                soundTouch.PutSamples(block, BLOCK_SIZE / 2);
                int receivedSamplesCount = (int)soundTouch.ReceiveSamples(processedBuffer, BLOCK_SIZE / 2);
                outBuffer.Write(processedBuffer, 0, receivedSamplesCount * 2);
            }
        }

        void ResetComponent()
        {
            audioReadPos = 0;
            soundTouch.Clear();
            inBuffer.Clear();
            outBuffer.Clear();
        }

        void PrintVersion()
        {
            Debug.Log("SoundTouch version: " + SoundTouch.Version);
        }
        #endregion
    }
}