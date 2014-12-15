// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Its.Configuration
{
    internal static class Conversion
    {
        public static readonly Dictionary<string, Func<string, object>> DefaultTypeConverters = CreateDefaultConverters();

        private static Dictionary<string, Func<string, object>> CreateDefaultConverters()
        {
            var converters = new Dictionary<string, Func<string, object>>();

            converters
                .AddConverterToNullable<bool>()
                .AddConverterToNullable<byte>()
                .AddConverterToNullable<DateTime>()
                .AddConverterToNullable<decimal>()
                .AddConverterToNullable<double>()
                .AddConverterToNullable<float>()
                .AddConverterToNullable<Int16>()
                .AddConverterToNullable<Int32>()
                .AddConverterToNullable<Int64>()
                .AddConverter(v => new Uri(v));

            converters
                .AddConverterToObservable<string>()
                .AddConverterToObservable<bool>()
                .AddConverterToObservable<byte>()
                .AddConverterToObservable<DateTime>()
                .AddConverterToObservable<decimal>()
                .AddConverterToObservable<double>()
                .AddConverterToObservable<float>()
                .AddConverterToObservable<Int16>()
                .AddConverterToObservable<Int32>()
                .AddConverterToObservable<Int64>();

            return converters;
        }

        public static Dictionary<string, Func<string, object>> AddConverter<T>(
            this Dictionary<string, Func<string, object>> typeConverters,
            Func<string, T> convert)
        {
            typeConverters.Add(typeof (T).Key(), s => convert(s));
            typeConverters.Add(typeof (IObservable<T>).Key(), value => new BehaviorSubject<T>(convert(value)));
            return typeConverters;
        }

        public static Dictionary<string, Func<string, object>> AddConverterToObservable<T>(this Dictionary<string, Func<string, object>> typeConverters)
        {
            Func<string, object> convert = value => Convert.ChangeType(value, typeof (T));
            typeConverters.Add(typeof (IObservable<T>).Key(), value => new BehaviorSubject<T>((T) convert(value)));
            return typeConverters;
        }

        public static Dictionary<string, Func<string, object>> AddConverterToNullable<T>(
            this Dictionary<string, Func<string, object>> typeConverters) where T : struct
        {
            return typeConverters.AddConverter(s => s.ToNullable<T>());
        }

        public static T? ToNullable<T>(this string s) where T : struct
        {
            var result = new T?();
            if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
            {
                var conv = TypeDescriptor.GetConverter(typeof (T));
                result = (T) conv.ConvertFrom(s);
            }
            return result;
        }
    }
}