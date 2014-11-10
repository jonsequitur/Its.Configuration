using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Its.Configuration
{
    [DebuggerStepThrough]
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return source.Select(item =>
            {
                action(item);
                return item;
            });
        }

        public static IEnumerable<TSource> Run<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    //  var current = enumerator.Current;
                }
            }
            return source;
        }

        public static void ForEach<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            source.Do(action).Run();
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source.ToArray());
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> items)
        {
            return items ?? Enumerable.Empty<T>();
        }
    }
}