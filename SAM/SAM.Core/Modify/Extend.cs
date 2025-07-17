using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        /// <summary>
        /// Extends list to given length (count) by adding values from start of the list
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="list">List</param>
        /// <param name="count">New length of the list</param>
        /// <returns>True if new items added. False if list is longer than given count value, list is empty or null</returns>
        public static bool Extend<T>(this List<T> list, int count)
        {
            if (list == null || list.Count == 0)
            {
                return false;
            }

            int currentCount = list.Count;

            if (currentCount >= count)
            {
                return false;
            }

            for (int i = currentCount; i < count; i++)
            {
                list.Add(list[i % currentCount]);
            }

            return true;
        }

    }
}