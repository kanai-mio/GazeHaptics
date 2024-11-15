// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;

namespace AudioStream
{
    /// <summary>
    /// An FMOD sound specific audio buffer for static PCM callbacks
    /// </summary>
    public class PCMCallbackBuffer
    {
        /// <summary>
        /// (UI only) Flag set in PCM callback indicating not enough data from OAFR
        /// </summary>
        public bool underflow = false;

        BasicBufferByte pcmReadCallback_Buffer = null;

        // lock on pcmReadCallbackBuffer - can be changed ( added to ) in OAFR thread leading to collisions
        readonly object pcmReadCallback_BufferLock = new object();

        // TODO: pass size computed from buffers sizes
        public PCMCallbackBuffer(uint capacity)
        {
            this.pcmReadCallback_Buffer = new BasicBufferByte(capacity);
        }

        public void Enqueue(byte[] bytes)
        {
            lock (this.pcmReadCallback_BufferLock)
            {
                this.pcmReadCallback_Buffer.Write(bytes);
            }
        }

        public byte[] Dequeue(uint requiredCount)
        {
            lock (this.pcmReadCallback_BufferLock)
            {
                var returnCount = System.Math.Min(requiredCount, this.pcmReadCallback_Buffer.Available());
                return this.pcmReadCallback_Buffer.Read(returnCount);
            }
        }

        public void Resize()
        {
            lock (this.pcmReadCallback_BufferLock)
            {
                var nsize = this.pcmReadCallback_Buffer.Capacity() * 2;
                if (nsize > uint.MaxValue)
                    // f this
                    return;
                var newb = new BasicBufferByte(nsize);
                var ex = this.pcmReadCallback_Buffer.Read(this.pcmReadCallback_Buffer.Available());
                newb.Write(ex);
                this.pcmReadCallback_Buffer = newb;
            }
        }

        public uint Available
        {
            get { return this.pcmReadCallback_Buffer.Available(); }
        }

        public uint Capacity
        {
            get { return this.pcmReadCallback_Buffer.Capacity(); }
        }
    }
}