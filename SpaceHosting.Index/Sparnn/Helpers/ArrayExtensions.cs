using System;
using System.Collections.Generic;

namespace SpaceHosting.Index.Sparnn.Helpers
{
    internal static class ArrayExtensions
    {
        public static List<(int, double)> TakeKBest(this double[] elements, int k)
        {
            var comparer = new PairComparer();

            var size = Math.Min(k, elements.Length);
            var temp = new List<(int, double)>(size + 1);
            for (var i = 0; i < size; i++)
            {
                temp.Add((i, elements[i]));
            }

            for (var i = temp.Count; i < elements.Length; i++)
            {
                if (!(elements[i] < temp[^1].Item2))
                {
                    continue;
                }

                var pair = (i, elements[i]);
                var index = temp.BinarySearch(pair, comparer);
                if (index < 0)
                {
                    temp.Insert(~index, pair);
                }
                else if (index < size)
                {
                    temp.Insert(index, pair);
                }

                temp.RemoveAt(temp.Count - 1);
            }

            return temp;
        }

        public static double JaccardBinaryDistance(this int[] v1, int[] v2)
        {
            var match = 0;
            var cardinality = 0;

            var i = 0;
            var j = 0;
            for (; i < v1.Length && j < v2.Length;)
            {
                var comp = v1[i].CompareTo(v2[j]);

                switch (comp)
                {
                    case 0:
                        match++;
                        i++;
                        j++;
                        break;
                    case -1:
                        i++;
                        break;
                    default:
                        j++;
                        break;
                }

                cardinality++;
            }

            if (i < v1.Length)
                cardinality += v1.Length - i;

            if (j < v2.Length)
                cardinality += v2.Length - j;

            var result = 1.0 - (double)match / cardinality;
            return result;
        }

        private class PairComparer : IComparer<(int, double)>
        {
            public int Compare((int, double) x, (int, double) y)
            {
                return x.Item2.CompareTo(y.Item2);
            }
        }
    }
}
