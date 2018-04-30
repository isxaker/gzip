using System;
using Helpers.Sort;

namespace Helpers.DataStructure
{

    public class BlockingSortableMinQueue<T> : BlockingQueue<T> where T : class, IComparable<T>
    {
        #region properties

        public T Min
        {
            get
            {
                lock (this.Queue)
                {
                    return ((SortableMinQueue<T>) this.Queue).CurrentMin;
                }
            }
        }

        #endregion

        #region  constructors

        public BlockingSortableMinQueue(int maxSize, bool autoUpdateMinAfterDequeue) : base(maxSize, new SortableMinQueue<T>(maxSize, autoUpdateMinAfterDequeue))
        {
        }

        #endregion

        public void RefreshMin()
        {
            lock (this.Queue)
            {
                ((SortableMinQueue<T>)this.Queue).RefreshMin();
            }
        }

        public void Sort(ISort<T> alg)
        {
            lock (this.Queue)
            {
                ((SortableMinQueue<T>)this.Queue).Sort(alg);
            }
        }

    }

}