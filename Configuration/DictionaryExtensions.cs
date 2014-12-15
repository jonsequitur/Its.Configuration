// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Its.Configuration
{
    internal static class DictionaryExtensions
    {
        /// <summary>
        ///   Gets the a value having the specified key from the dictionary. If it is not present, the supplied function is called and the result is added to the dictionary and returned.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> orElse)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            value = orElse(key);
            dictionary.Add(key, value);
            return value;
        }
    }
}