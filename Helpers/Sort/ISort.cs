using System;

namespace Helpers.Sort
{

    public interface ISort<in T> where T : IComparable<T>
    {

        void Sort(T[] arr, int count);

    }

}