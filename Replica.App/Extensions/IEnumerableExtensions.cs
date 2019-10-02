using System;
using System.Collections.Generic;
using System.Linq;

namespace Replica.App.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> collection)
        {
            return collection.ElementAt(new Random().Next( collection.Count()));
        }
    }
}
