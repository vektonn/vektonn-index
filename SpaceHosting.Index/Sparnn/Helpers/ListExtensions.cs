using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceHosting.Index.Sparnn.Helpers
{
    internal static class ListExtensions
    {
        public static IEnumerable<T[]> HStack<T>(this IEnumerable<T[]>[] blocks)
        {
            var enumerators = blocks.Select(b => b.GetEnumerator()).ToArray();

            try
            {
                while (true)
                {
                    var escaping = false;
                    var currentRow = new List<T>();

                    foreach (var enumerator in enumerators)
                    {
                        if (!enumerator.MoveNext())
                        {
                            escaping = true;
                            break;
                        }

                        var row = enumerator.Current;
                        currentRow.AddRange(row);
                    }

                    if (escaping) break;
                    yield return currentRow.ToArray();
                }
            }
            finally
            {
                Exception e = null;

                foreach (var enumerator in enumerators)
                {
                    if (e is null && enumerator.MoveNext())
                        e = new Exception("blocks have different rows count!");

                    enumerator.Dispose();
                }

                if (e != null)
                    throw e;
            }
        }

        public static T[] TakeKBest<T>(this IList<T> elements, int k, Func<T, double> keySelector)
        {
            if (k <= 0)
                throw new ArgumentException("k must be positive");

            var temp = elements.Take(k).ToList();
            temp.Sort();

            for (var i = temp.Count; i < elements.Count; i++)
            {
                if (keySelector(elements[i]) < keySelector(temp[temp.Count - 1]))
                {
                    var index = temp.BinaryFindIndex(elements[i], keySelector);
                    temp.Insert(index, elements[i]);
                    temp.RemoveAt(temp.Count - 1);
                }
            }

            return temp.ToArray();
        }

        private static int BinaryFindIndex<T>(this IList<T> sortedList, T element, Func<T, double> keySelector)
        {
            var left = 0;
            var right = sortedList.Count - 1;

            while (left <= right)
            {
                var middle = (left + right) / 2;

                if (keySelector(sortedList[middle]) > keySelector(element))
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            return left;
        }
    }
}
