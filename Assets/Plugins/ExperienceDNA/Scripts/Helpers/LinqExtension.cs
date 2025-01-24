using System.Collections.Generic;

namespace MICT.eDNA.Helpers
{
    public static class LinqExtension
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumeration, System.Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
                yield return item;
            }
        }
    } 
}