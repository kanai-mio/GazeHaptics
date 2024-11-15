// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.

// BasicBuffer from ConcentusDemo
// (c) and license see LICENSE in AudioStream/Scripts/Codecs/Concentus/LICENSE
using System;

// using System.IO;

namespace AudioStreamSupport
{
    public class BasicBuffer<T>
    {
        private T[] data;
        private uint writeIndex = 0;
        private uint readIndex = 0;
        private uint available = 0;
        private uint capacity = 0;

        public BasicBuffer(uint capacity)
        {
            this.capacity = capacity;
            data = new T[capacity];
        }

        public void Write(T[] toWrite)
        {
            // Write the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < toWrite.Length)
            {
                var count = Math.Min(toWrite.Length - sourceIndex, capacity - writeIndex);
                Array.Copy(toWrite, sourceIndex, data, writeIndex, count);
                writeIndex = (uint)(writeIndex + count) % capacity;
                sourceIndex += (uint)count;
            }
            available += (uint)toWrite.Length;
            // Did we overflow? In this case, move the readIndex to just after the writeIndex
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        /// <summary>
        /// Writes a single value
        /// </summary>
        /// <param name="toWrite"></param>
        public void Write(T toWrite)
        {
            data[writeIndex] = toWrite;
            writeIndex = (writeIndex + 1) % capacity;
            available += 1;
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        public void Clear()
        {
            writeIndex = 0;
            readIndex = 0;
            available = 0;
        }

        public T[] Read(uint count)
        {
            T[] returnVal = new T[count];
            // Read the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < count)
            {
                var readCount = Math.Min(count - sourceIndex, capacity - readIndex);
                Array.Copy(data, readIndex, returnVal, sourceIndex, readCount);
                readIndex = (readIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            available -= count;
            // Did we underflow? In this case, move the writeIndex to where the next data will be read
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads a single value
        /// </summary>
        /// <returns></returns>
        public T Read()
        {
            T returnVal = data[readIndex];
            readIndex = (readIndex + 1) % capacity;
            available -= 1;
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads from the buffer without actually consuming the data
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public T[] Peek(uint count)
        {
            var toRead = Math.Min(available, count);
            T[] returnVal = new T[toRead];
            // Read the data in chunks
            uint sourceIndex = 0;
            uint localReadIndex = readIndex;
            while (sourceIndex < toRead)
            {
                var readCount = Math.Min(toRead - sourceIndex, capacity - localReadIndex);
                Array.Copy(data, localReadIndex, returnVal, sourceIndex, readCount);
                localReadIndex = (localReadIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            return returnVal;
        }

        public uint Available()
        {
            return available;
        }

        public uint Capacity()
        {
            return capacity;
        }
    }

    /// <summary>
    /// A drop-in implementation of BasicBuffer strongly typed to the int16 data.
    /// This has better performance in the CLR because it avoids the boxed arrays used in the generic buffer class
    /// </summary>
    public class BasicBufferShort
    {
        private short[] data;
        private uint writeIndex = 0;
        private uint readIndex = 0;
        private uint available = 0;
        private uint capacity = 0;

        public BasicBufferShort(uint capacity)
        {
            this.capacity = capacity;
            data = new short[capacity];
        }

        public void Write(short[] toWrite)
        {
            // Write the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < toWrite.Length)
            {
                var count = Math.Min(toWrite.Length - sourceIndex, capacity - writeIndex);
                Array.Copy(toWrite, sourceIndex, data, writeIndex, count);
                writeIndex = (uint)(writeIndex + count) % capacity;
                sourceIndex += (uint)count;
            }
            available += (uint)toWrite.Length;
            // Did we overflow? In this case, move the readIndex to just after the writeIndex
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        /// <summary>
        /// Writes a single value
        /// </summary>
        /// <param name="toWrite"></param>
        public void Write(short toWrite)
        {
            data[writeIndex] = toWrite;
            writeIndex = (writeIndex + 1) % capacity;
            available += 1;
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        public void Clear()
        {
            writeIndex = 0;
            readIndex = 0;
            available = 0;
        }

        public short[] Read(uint count)
        {
            short[] returnVal = new short[count];
            // Read the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < count)
            {
                var readCount = Math.Min(count - sourceIndex, capacity - readIndex);
                Array.Copy(data, readIndex, returnVal, sourceIndex, readCount);
                readIndex = (readIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            available -= count;
            // Did we underflow? In this case, move the writeIndex to where the next data will be read
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads a single value
        /// </summary>
        /// <returns></returns>
        public short Read()
        {
            short returnVal = data[readIndex];
            readIndex = (readIndex + 1) % capacity;
            available -= 1;
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads from the buffer without actually consuming the data
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public short[] Peek(uint count)
        {
            uint toRead = Math.Min(available, count);
            short[] returnVal = new short[toRead];
            // Read the data in chunks
            uint sourceIndex = 0;
            uint localReadIndex = readIndex;
            while (sourceIndex < toRead)
            {
                var readCount = Math.Min(toRead - sourceIndex, capacity - localReadIndex);
                Array.Copy(data, localReadIndex, returnVal, sourceIndex, readCount);
                localReadIndex = (localReadIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            return returnVal;
        }

        public uint Available()
        {
            return available;
        }

        public uint Capacity()
        {
            return capacity;
        }
    }

    /// <summary>
    /// A drop-in implementation of BasicBuffer strongly typed to the byte data.
    /// This has better performance in the CLR because it avoids the boxed arrays used in the generic buffer class
    /// </summary>
    public class BasicBufferByte
    {
        private byte[] data;
        private uint writeIndex = 0;
        private uint readIndex = 0;
        private uint available = 0;
        private uint capacity = 0;

        // Debug RAW save
        // FileStream fs;
        // BinaryWriter bw;
        public BasicBufferByte(uint capacity)
        {
            this.capacity = capacity;
            data = new byte[capacity];
            //this.fs = new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), string.Format("Binary{0}.RAW", Path.GetFileName(Path.GetTempFileName())))
            //    , FileMode.Create, FileAccess.Write, FileShare.None)
            //    ;
            //this.bw = new BinaryWriter(this.fs);
        }

        //public void Close()
        //{
        //    this.bw.Close();
        //    this.fs.Close();
        //}

        public void Write(byte[] toWrite)
        {
            // this.bw.Write(toWrite);

            // Write the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < toWrite.Length)
            {
                var count = Math.Min(toWrite.Length - sourceIndex, capacity - writeIndex);
                Array.Copy(toWrite, sourceIndex, data, writeIndex, count);
                writeIndex = (uint)(writeIndex + count) % capacity;
                sourceIndex += (uint)count;
            }
            available += (uint)toWrite.Length;
            // Did we overflow? In this case, move the readIndex to just after the writeIndex
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        /// <summary>
        /// Writes a single value
        /// </summary>
        /// <param name="toWrite"></param>
        public void Write(byte toWrite)
        {
            data[writeIndex] = toWrite;
            writeIndex = (writeIndex + 1) % capacity;
            available += 1;
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        public void Clear()
        {
            writeIndex = 0;
            readIndex = 0;
            available = 0;
        }

        public byte[] Read(uint count)
        {
            byte[] returnVal = new byte[count];
            // Read the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < count)
            {
                var readCount = Math.Min(count - sourceIndex, capacity - readIndex);
                Array.Copy(data, readIndex, returnVal, sourceIndex, readCount);
                readIndex = (uint)(readIndex + readCount) % capacity;
                sourceIndex += (uint)readCount;
            }
            available -= count;
            // Did we underflow? In this case, move the writeIndex to where the next data will be read
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads a single value
        /// </summary>
        /// <returns></returns>
        public byte Read()
        {
            byte returnVal = data[readIndex];
            readIndex = (readIndex + 1) % capacity;
            available -= 1;
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads from the buffer without actually consuming the data
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Peek(uint count)
        {
            var toRead = Math.Min(available, count);
            byte[] returnVal = new byte[toRead];
            // Read the data in chunks
            uint sourceIndex = 0;
            uint localReadIndex = readIndex;
            while (sourceIndex < toRead)
            {
                var readCount = Math.Min(toRead - sourceIndex, capacity - localReadIndex);
                Array.Copy(data, localReadIndex, returnVal, sourceIndex, readCount);
                localReadIndex = (localReadIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            return returnVal;
        }

        public uint Available()
        {
            return available;
        }

        public uint Capacity()
        {
            return capacity;
        }
    }

    /// <summary>
    /// A drop-in implementation of BasicBuffer strongly typed to the byte data.
    /// This has better performance in the CLR because it avoids the boxed arrays used in the generic buffer class
    /// </summary>
    public class BasicBufferFloat
    {
        private float[] data;
        private uint writeIndex = 0;
        private uint readIndex = 0;
        private uint available = 0;
        private uint capacity = 0;

        public BasicBufferFloat(uint capacity)
        {
            this.capacity = capacity;
            data = new float[capacity];
        }

        public void Write(float[] toWrite)
        {
            // Write the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < toWrite.Length)
            {
                var count = Math.Min(toWrite.Length - sourceIndex, capacity - writeIndex);
                Array.Copy(toWrite, sourceIndex, data, writeIndex, count);
                writeIndex = (uint)(writeIndex + count) % capacity;
                sourceIndex += (uint)count;
            }
            available += (uint)toWrite.Length;
            // Did we overflow? In this case, move the readIndex to just after the writeIndex
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        /// <summary>
        /// Writes a single value
        /// </summary>
        /// <param name="toWrite"></param>
        public void Write(float toWrite)
        {
            data[writeIndex] = toWrite;
            writeIndex = (writeIndex + 1) % capacity;
            available += 1;
            if (available > capacity)
            {
                readIndex = (writeIndex + 1) % capacity;
                available = capacity;
            }
        }

        public void Clear()
        {
            writeIndex = 0;
            readIndex = 0;
            available = 0;
        }

        public float[] Read(uint count)
        {
            float[] returnVal = new float[count];
            // Read the data in chunks
            uint sourceIndex = 0;
            while (sourceIndex < count)
            {
                var readCount = Math.Min(count - sourceIndex, capacity - readIndex);
                Array.Copy(data, readIndex, returnVal, sourceIndex, readCount);
                readIndex = (readIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            available -= count;
            // Did we underflow? In this case, move the writeIndex to where the next data will be read
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads a single value
        /// </summary>
        /// <returns></returns>
        public float Read()
        {
            float returnVal = data[readIndex];
            readIndex = (readIndex + 1) % capacity;
            available -= 1;
            if (available < 0)
            {
                writeIndex = (readIndex + 1) % capacity;
                available = 0;
            }
            return returnVal;
        }

        /// <summary>
        /// Reads from the buffer without actually consuming the data
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Peek(uint count)
        {
            var toRead = Math.Min(available, count);
            byte[] returnVal = new byte[toRead];
            // Read the data in chunks
            uint sourceIndex = 0;
            uint localReadIndex = readIndex;
            while (sourceIndex < toRead)
            {
                var readCount = Math.Min(toRead - sourceIndex, capacity - localReadIndex);
                Array.Copy(data, localReadIndex, returnVal, sourceIndex, readCount);
                localReadIndex = (localReadIndex + readCount) % capacity;
                sourceIndex += readCount;
            }
            return returnVal;
        }

        public uint Available()
        {
            return available;
        }

        public uint Capacity()
        {
            return capacity;
        }
    }
}