using System;

namespace Helpers.Sort
{

    public class ShellSort<T> : ISort<T> where T : IComparable<T>
    {

        public void Sort(T[] arr, int count)
        {
            int n = count;
            int h = 1;
            while (h < n/3)
                h = 3*h + 1;

            while (h >= 1)
            {
                // h-sort the array
                for (int i = h; i < n; i++)
                {
                    for (int j = i; j >= h && arr[j].CompareTo(arr[j - h]) < 0; j -= h)
                    {
                        this.Swap(arr, j, j - h);
                    }
                }
                h /= 3;
            }
        }

        private void Swap(T[] arr, int i, int j)
        {
            if (i == j)
            {
                return;
            }
            T temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

    }

}