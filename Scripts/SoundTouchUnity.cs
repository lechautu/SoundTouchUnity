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
        int audioReadPosition;

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

        float pitchSemitone = 0;
        public float PitchSemitone
        {
            get => pitchSemitone;
            set
            {
                pitchSemitone = value;
                SetupSemitone();
            }
        }

        int delayNumSamples;
        public int DelayNumSamples
        {
            get => delayNumSamples;
            set
            {
                delayNumSamples = value;
                Debug.Log("DelayNumSamples: " + delayNumSamples);
            }
        }

        public bool IsPlaying => audioSource.isPlaying && audioInputData != null && (audioReadPosition - delayNumSamples) < audioInputData.Length;
        public float Duration => audioClip != null ? audioClip.length : 0;
        public float Time => audioClip != null && audioInputData != null ? 1.0f * (audioReadPosition - delayNumSamples) / audioInputData.Length * audioClip.length : 0;

        public void Play()
        {
            if (audioClip == null)
            {
                Debug.LogWarning("clip is not set");
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
            audioSource.Stop();
            Flush(true, true);
        }

        public void Seek(float seconds)
        {
            seconds = Mathf.Clamp(seconds, 0, audioClip.length);
            audioReadPosition = (int)(seconds / audioClip.length * audioInputData.Length);
            Flush(clearBuffer: true);
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
            audioSource.Stop();
            Flush(resetAudioReadPosition: true);
            if (audioClip == null)
            {
                return;
            }

            audioInputData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioInputData, 0);
            int channels = audioClip.channels;
            int freq = audioClip.frequency;
            soundTouch.Channels = (uint)channels;
            soundTouch.SampleRate = (uint)freq;
            audioSource.clip = CreateDynamicClip(channels, freq, ref dynamicClip);
            DelayNumSamples = audioReadPosition;
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

        void SetupSemitone()
        {
            if (soundTouch == null)
            {
                return;
            }
            soundTouch.PitchSemiTones = pitchSemitone;
        }

        void PCMReaderCallback(float[] data)
        {
            int needData = data.Length;
            while (outBuffer.Available < needData && audioReadPosition < audioInputData.Length)
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
            Debug.Log("PCMReaderCallback");
        }

        void Process(int length)
        {
            if (inBuffer == null || outBuffer == null)
            {
                return;
            }

            int readLength = Mathf.Min(length, audioInputData.Length - audioReadPosition);
            inBuffer.Write(audioInputData, audioReadPosition, readLength);
            audioReadPosition += readLength;

            while (inBuffer.Available >= BLOCK_SIZE)
            {
                var block = new float[BLOCK_SIZE];
                inBuffer.Read(block, 0, block.Length);
                soundTouch.PutSamples(block, BLOCK_SIZE / 2);
                int receivedSamplesCount = (int)soundTouch.ReceiveSamples(processedBuffer, BLOCK_SIZE / 2);
                outBuffer.Write(processedBuffer, 0, receivedSamplesCount * 2);
            }
        }

        void Flush(bool clearBuffer = false, bool resetAudioReadPosition = false)
        {
            if (resetAudioReadPosition)
            {
                audioReadPosition = 0;
            }
            soundTouch.Clear();
            inBuffer.Clear();
            outBuffer.Clear();
            if (clearBuffer)
            {
                audioSource.clip = CreateDynamicClip(audioClip.channels, audioClip.frequency, ref dynamicClip);
                if (resetAudioReadPosition)
                {
                    DelayNumSamples = audioReadPosition;
                }
            }
        }

        void PrintVersion()
        {
            if (SoundTouch.IsAvailable)
                Debug.Log("SoundTouch version: " + SoundTouch.Version);
        }
        #endregion
    }
}