using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IWalker.Util
{
    /// <summary>
    /// Some helper functions to make dealing with lists that have to be updated simpler.
    /// This reminds me of the old emacs update algorithm, that optimized for 300 baud modems.
    /// </summary>
    public static class ListOrdering
    {
        /// <summary>
        /// Returns a new list that looks like the desired list. If any of the objects in desired are already in
        /// the original, the original objects will be re-used. object.Equals is what is used to do this.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note that the original and desired are made into in-memory arrays. This algorithm is not tuned for
        /// very large datasets!
        /// </remarks>
        public static IEnumerable<T> MakeLookLike<T> (this IEnumerable<T> original, IEnumerable<T> desired)
            where T : IEquatable<T>
        {
            return original.MakeLookLike(desired, (oItem, dItem) => oItem.Equals(dItem), dItem => dItem);
        }

        /// <summary>
        /// Make the original list look like the desired list. The two can contain different types, as long as the original type can be generated
        /// by the desired type and a comparison function is supplied.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="desired"></param>
        /// <param name="compare"></param>
        /// <param name="generate"></param>
        /// <returns></returns>
        public static IEnumerable<U> MakeLookLike<U, T> (this IEnumerable<U> original, IEnumerable<T> desired, Func<U, T, bool> compare, Func<T, U> generate)
        {
            // Find anything in desired that is already in original.
            var oArray = original.ToArray();
            var dArray = desired.ToArray();
            var r = from oIndex in Enumerable.Range(0, oArray.Count())
                    let oItem = oArray[oIndex]
                    let dMatch = dArray.Where(dItem => compare(oItem, dItem)).FirstOrDefault()
                    where dMatch != null
                    select Tuple.Create(dMatch, oIndex);
            var desiredToOriginalMapping = r.ToDictionary(info => info.Item1, info => info.Item2);

            // Where we have a desired item, return that. Otherwise, the other guy.
            return dArray
                .Select(dItem =>
                {
                    if (desiredToOriginalMapping.ContainsKey(dItem))
                    {
                        return oArray[desiredToOriginalMapping[dItem]];
                    }
                    else
                    {
                        return generate(dItem);
                    }
                });
        }
    }
}
