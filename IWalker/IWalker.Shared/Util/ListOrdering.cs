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
        public static IEnumerable<T> MakeLookLike<T>(this IEnumerable<T> original, IEnumerable<T> desired)
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
        public static IEnumerable<U> MakeLookLike<U, T>(this IEnumerable<U> original, IEnumerable<T> desired, Func<U, T, bool> compare, Func<T, U> generate)
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

        /// <summary>
        /// Using as few operations as possible, make the original list look like the desired list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        public static void MakeListLookLike<T>(this IList<T> original, IEnumerable<T> desired)
            where T : class, IEquatable<T>
        {
            original.MakeListLookLike(desired, (oItem, dItem) => oItem.Equals(dItem), dItem => dItem);
        }

        /// <summary>
        /// Execute operations on the IList to make it look like the desired list.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="desired"></param>
        /// <param name="compare"></param>
        /// <param name="generate"></param>
        public static void MakeListLookLike<U, T>(this IList<U> original, IEnumerable<T> desired, Func<U, T, bool> compare, Func<T, U> generate)
            where T : class
            where U : class
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
            // Note that the ToArray below has to be here because the original list, and the
            // oArray, are changing as we walk through the list.
            var me = dArray
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
                })
                .ToArray();

            // Loop over the two arrays, updating the IList to look like our final list.
            int index = 0;
            foreach (var item in me)
            {
                // If we are out beyond the end of the list... then just do an insert!
                if (index >= original.Count)
                {
                    original.Add(item);
                    index++;
                }
                else if (oArray[index] != item)
                {
                    // We need to put this item here, but if it is further down, perhaps we can just delete to it?
                    // If the item is further down, then we should remove everything until this item.
                    // TODO: an item swap can cause the whole list to be deleted here.
                    while (itemExistsAhead(oArray, index, item))
                    {
                        original.RemoveAt(index);
                        oArray = original.ToArray();
                    }

                    original.Insert(index, item);
                    oArray = original.ToArray();
                    index++;
                }
                else
                {
                    // The item is a match! Ignore it and pass on by.
                    index++;
                }
            }

            // Clean out anything that is left over at the end of the list!
            while (index < original.Count)
            {
                original.RemoveAt(index);
            }
        }

        /// <summary>
        /// The item exists somewhere further down the list. It does not exist at oArray[index].
        /// Works even if there are no items left.
        /// </summary>
        /// <param name="oArray"></param>
        /// <param name="startPoint"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool itemExistsAhead<U>(U[] oArray, int startPoint, object item)
            where U : class
        {
            for (int i = startPoint + 1; i < oArray.Length; i++)
            {
                if (oArray[i] == item)
                    return true;
            }

            return false;
        }
    }
}
