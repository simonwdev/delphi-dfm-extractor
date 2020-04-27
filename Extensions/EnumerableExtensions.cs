using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Extensions
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> f)
        {
            return source.SelectMany(c => f(c).Flatten(f)).Concat(source);
        }

        public static HashSet<TElement> ToHashSet<TSource, TElement>(this IEnumerable<TSource> source, Func<TSource, TElement> elementSelector, IEqualityComparer<TElement> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            return new HashSet<TElement>(source.Select(elementSelector), comparer);
        }
    }
}
