using System;
using Helpers.Sort;

namespace Helpers.DataStructure
{

    public class SortableMinQueue<T> : Queue<T> where T : class, IComparable<T>
    {
        #region  fields

        private T _currentMin;
        private readonly bool _autoUpdateMinAfterDequeue;

        #endregion

        #region properties

        public T CurrentMin
        {
            get
            {
                if (this.IsEmpty())
                {
                    throw new InvalidOperationException("Queue is empty");
                }
                return this._currentMin;
            }
        }

        #endregion

        #region  constructors

        public SortableMinQueue(int size, bool autoUpdateMinAfterDequeue) : base(size)
        {
            this._autoUpdateMinAfterDequeue = autoUpdateMinAfterDequeue;
            this._currentMin = default(T);
        }

        #endregion

        #region public methods

        public override void Enqueue(T item)
        {
            base.Enqueue(item);
            this.UpdateMin(item);
        }

        public override T Dequeue()
        {
            T removed = base.Dequeue();
            if (this._autoUpdateMinAfterDequeue && removed.Equals(_currentMin))
            {
                this.RefreshMin();
            }
            return removed;
        }

        public void RefreshMin()
        {
            if (this.IsEmpty())
            {
                this._currentMin = default(T);
            }

            this._currentMin = this.Data[Head];
            int itemIndex = Head, j = 0;
            while (j < this.Count)
            {
                UpdateMin(this.Data[itemIndex]);
                MoveNext(ref itemIndex);
                j++;
            }
        }

        public void Sort(ISort<T> alg)
        {
            this.Rebuild();
            alg.Sort(this.Data, this.Count);
            //Array.Sort(this._data, this._head, this.Count, comparer);
        }

        #endregion

        #region helpers methods

        //rebuilding array to view where head = 0, and tail = count - 1
        private void Rebuild()
        {
            // if queals zero than the deals've already been done
            if (this.Head != 0)
            {
                int destinationStartIndex, sourceStartIndex, count;
                //just moving from the head to array index 0
                if (this.Head < this.Tail)
                {
                    destinationStartIndex = 0;
                    sourceStartIndex = this.Head;
                    count = this.Count;
                }
                else //moving from the head to the tail
                {
                    destinationStartIndex = this.Tail;
                    sourceStartIndex = this.Head;
                    count = this.Size - this.Head;
                }

                //moving and update
                this.Moving(destinationStartIndex, sourceStartIndex, count);
                this.Head = 0;
                this.Tail = this.Count == this.Size ? 0 : this.Count;
            }
        }

        private void Moving(int destinationStartIndex, int sourceStartIndex, int count)
        {
            if (destinationStartIndex == sourceStartIndex)
            {
                return;
            }
            while (count > 0)
            {
                this.Data[destinationStartIndex] = this.Data[sourceStartIndex];
                this.Data[sourceStartIndex] = default(T);
                destinationStartIndex++;
                sourceStartIndex++;
                count--;
            }
        }

        private void UpdateMin(T item)
        {
            if (this.Count == 1)
            {
                this._currentMin = item;
            }

            this._currentMin = this._currentMin.CompareTo(item) > 0 ? item : this._currentMin;
        }

        #endregion

    }

}