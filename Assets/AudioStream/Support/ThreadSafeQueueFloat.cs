// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.

using System.Collections.Generic;

namespace AudioStreamSupport
{
    public class ThreadSafeQueueFloat
    {
        readonly Queue<float> queue = new Queue<float>();

        public int Write(float[] data)
        {
            lock (this.queue)
            {
                var length = data.Length;
                for (var i = 0; i < length; ++i)
                    this.queue.Enqueue(data[i]);
                return length;
            }
        }

        public int Read(float[] data)
        {
            lock (this.queue)
            {
                var length = data.Length;
                for (var i = 0; i < length; ++i)
                {
                    if (this.queue.Count > 0)
                        data[i] = this.queue.Dequeue();
                    else
                        return i;
                    //if (!this.queue.TryDequeue(out data[i]))
                    //    return i;
                }

                return length;
            }
        }
        public void Reset()
        {
            lock (this.queue)
                this.queue.Clear();
        }

        public int Count => this.queue.Count;
    }
}