using System.Collections.Generic;

namespace SpaceHosting.Index.Sparnn.Helpers
{
    internal static class TupleExtensions
    {
        public static void Deconstruct<TX, TY>(this IEnumerable<(TX, TY)> arrayOfTuples, out TX[] arrayX, out TY[] arrayY)
        {
            var listOfX = new List<TX>();
            var listOfY = new List<TY>();

            foreach (var (itemX, itemY) in arrayOfTuples)
            {
                listOfX.Add(itemX);
                listOfY.Add(itemY);
            }

            arrayX = listOfX.ToArray();
            arrayY = listOfY.ToArray();
        }
    }
}
