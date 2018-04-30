using System;
using System.Collections.Generic;

namespace GZipTest.Core
{

    /// <summary>
    ///     Block of bytes with index
    /// </summary>
    public class ZipBlock : IComparable<ZipBlock>
    {
        #region  fields

        private readonly byte[] _data;
        private readonly long _index;

        #endregion

        #region properties

        public byte[] Data { get { return this._data; } }
        public long Index { get { return this._index; } }

        #endregion

        #region  constructors

        public ZipBlock(byte[] data, long index)
        {
            this._data = data;
            this._index = index;
        }

        public ZipBlock(byte[] data)
        {
            this._data = data;
        }

        #endregion

        #region IComparable<T>

        public int CompareTo(ZipBlock other)
        {
            return this.Index.CompareTo(other.Index);
        }

        #endregion
    }

    /// <summary>
    ///     Comparing ZipBlock's instances in scending order by Index field
    /// </summary>
    public class ZipBlockComparer : IComparer<ZipBlock>
    {

        public int Compare(ZipBlock x, ZipBlock y)
        {
            return x.Index.CompareTo(y.Index);
        }

    }

}