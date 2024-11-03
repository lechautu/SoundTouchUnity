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

        public float Duration => audioClip != null ? audioClip.length : 0;

        private bool isSourcePlaying;
        public bool IsPlaying => audioInputData != null && audioReadPos < audioInputData.Length && isSourcePlaying;

        public float Time => audioInputData != null && audioClip != null ? 1.0f * audioReadPos / audioInputData.Length * audioClip.length : 0;

        public void Play()
        {
            if (audioClip == null)
            {
                Debug.LogWarning("clip is not set");
                return;
            }
            if (IsPlaying)
            {
                return;
            }
            audioSource.Play();
        }

        public void Pause()
        {
            audioSource.Pause();
        }

        public void Stop()
        {
            ResetComponent();
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
            SetupTempo();
        }

        void SetupClip()
        {
            if (audioClip == null)
            {
                return;
            }
            audioInputData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioInputData, 0);
            if (dynamicClip == null || dynamicClip.frequency != audioClip.frequency || dynamicClip.channels != audioClip.channels)
            {
                if (dynamicClip != null)
                {
                    Destroy(dynamicClip);
                }
                int channels = audioClip.channels;
                int freq = audioClip.frequency;
                dynamicClip = AudioClip.Create(string.Empty, 1024, channels, freq, true, PCMReaderCallback);
                audioSource.clip = dynamicClip;
                soundTouch.Channels = (uint)channels;
                soundTouch.SampleRate = (uint)freq;
            }
            ResetComponent();
        }

        void SetupTempo()
        {
            soundTouch.Tempo = tempo;
        }

        void PCMReaderCallback(float[] data)
        {
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
            audioSource.Stop();
        }

        void OnDestroy()
        {
            soundTouch.Dispose();
            inBuffer.Dispose();
            outBuffer.Dispose();
        }

        void PrintVersion()
        {
            Debug.Log("SoundTouch version: " + SoundTouch.Version);
        }

        void Update()
        {
            isSourcePlaying = audioSource.isPlaying;
        }
        #endregion
    }
}