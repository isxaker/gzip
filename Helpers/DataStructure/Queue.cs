using System;
using Helpers.Sort;

namespace Helpers.DataStructure
{

    public class Queue<T>
    {
        #region  fields

        protected readonly T[] Data;
        protected readonly int Size;
        protected int Head;
        protected int Tail;

        #endregion

        #region properties

        public int Count { get; private set; }

        #endregion

        #region  constructors

        public Queue(int size)
        {
            this.Size = size;
            this.Data = new T[size];
            this.Head = 0;
            this.Tail = 0;
        }

        #endregion

        public bool IsEmpty()
        {
            return this.Count == 0;
        }

        public bool IsFull()
        {
            return this.Count == this.Size;
        }

        public virtual void Enqueue(T item)
        {
            if (this.IsFull())
            {
                throw new InvalidOperationException("Queue full");
            }
            this.Data[this.Tail] = item;
            this.MoveNext(ref this.Tail);
            this.Count++;
        }

        public virtual T Dequeue()
        {
            if (this.IsEmpty())
            {
                throw new InvalidOperationException("Queue empty");
            }
            T removed = this.Data[this.Head];
            this.Data[this.Head] = default(T);
            this.MoveNext(ref this.Head);
            this.Count--;

            return removed;
        }

        public T Peek()
        {
            if (this.IsEmpty())
            {
                throw new InvalidOperationException("Queue empty");
            }

            return this.Data[this.Head];
        }

        #region helpers methods

        // Increments the index wrapping it if necessary.
        protected void MoveNext(ref int index)
        {
            // It is tempting to use the remainder operator here but it is actually much slower 
            // than a simple comparison and a rarely taken branch.   
            int tmp = index + 1;
            index = (tmp == this.Data.Length) ? 0 : tmp;
        }

        #endregion
    }

}