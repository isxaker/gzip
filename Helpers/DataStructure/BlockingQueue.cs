using System;
using System.Threading;
using Helpers.Sort;

namespace Helpers.DataStructure
{

    public class BlockingQueue<T>
    {
        #region  fields

        protected readonly int MaxSize;
        protected readonly Queue<T> Queue;
        protected bool Closing;
        #endregion

        #region properties

        public int Count
        {
            get
            {
                lock (this.Queue)
                {
                    return this.Queue.Count;
                }
            }
        }

        #endregion

        #region  constructors

        public BlockingQueue(int maxSize)
        {
            this.Queue = new Queue<T>(maxSize);
            this.MaxSize = maxSize;
        }

        protected BlockingQueue(int maxSize, Queue<T> queue)
        {
            this.MaxSize = maxSize;
            this.Queue = queue;
        }

        #endregion

        #region public methods

        public T Peek()
        {
            lock (this.Queue)
            {
                T item = this.Queue.Peek();
                return item;
            }
        }

        //public bool TryPeek(out T value)
        //{
        //    lock (this.Queue)
        //    {
        //        while (this.Queue.Count == 0)
        //        {
        //            if (this.Closing)
        //            {
        //                value = default(T);
        //                return false;
        //            }
        //            Monitor.Wait(this.Queue);
        //        }
        //        value = this.Queue.Peek();
        //        return true;
        //    }
        //}

        public void Enqueue(T item)
        {
            lock (this.Queue)
            {
                while (this.Queue.Count >= this.MaxSize)
                {
                    Monitor.Wait(this.Queue);
                }
                this.Queue.Enqueue(item);
                if (this.Queue.Count == 1)
                {
                    // wake up any blocked dequeue  
                    Monitor.PulseAll(this.Queue);
                }
            }
        }

        public T Dequeue()
        {
            lock (this.Queue)
            {
                while (this.Queue.Count == 0)
                {
                    Monitor.Wait(this.Queue);
                }
                T item = this.Queue.Dequeue();
                if (this.Queue.Count == this.MaxSize - 1)
                {
                    // wake up any blocked enqueue  
                    Monitor.PulseAll(this.Queue);
                }
                return item;
            }
        }

        public void Close()
        {
            lock (this.Queue)
            {
                this.Closing = true;
                Monitor.PulseAll(this.Queue);
            }
        }

        public bool TryDequeue(out T value)
        {
            lock (this.Queue)
            {
                while (this.Queue.Count == 0)
                {
                    if (this.Closing)
                    {
                        value = default(T);
                        return false;
                    }
                    Monitor.Wait(this.Queue);
                }
                value = this.Queue.Dequeue();
                if (this.Queue.Count == this.MaxSize - 1)
                {
                    // wake up any blocked enqueue
                    Monitor.PulseAll(this.Queue);
                }
                return true;
            }
        }

        #endregion
    }

}