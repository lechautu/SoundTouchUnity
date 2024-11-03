using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TL.SoundTouch
{
    public class CircularBuffer : IDisposable
    {
        const int DEFAULT_BUFFER_SIZE = 8192;

        float[] buffer;
        int writePos;
        int readPos;
        
        public int Available => (writePos - readPos + buffer.Length) % buffer.Length;

        public CircularBuffer() : this(DEFAULT_BUFFER_SIZE)
        {
            
        }

        public CircularBuffer(int size)
        {
            this.buffer = new float[size];
            this.writePos = 0;
            this.readPos = 0;
        }

        ~CircularBuffer()
        {
            Dispose();
        }

        public void Write(float[] data, int offset, int count)
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count must be less than buffer size");
            }

            for (int i = 0; i < count; i++)
            {
                this.buffer[this.writePos] = data[offset + i];
                this.writePos = (this.writePos + 1) % buffer.Length;
            }
        }

        public void Read(float[] data, int offset, int count)
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count must be less than buffer size");
            }

            for (int i = 0; i < count; i++)
            {
                data[offset + i] = this.buffer[this.readPos];
                this.readPos = (this.readPos + 1) % buffer.Length;
            }
        }

        public void Dispose()
        {
            this.buffer = null;
        }
    }
}
