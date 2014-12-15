// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;

namespace Its.Configuration.Tests
{
    public static class AssertExtensions
    {
        public static bool IsCorrectlyConfigured(this SomethingConfigurable obj)
        {
            Assert.That(obj.ABool, Is.EqualTo(bool.Parse(ConfigurationManager.AppSettings["some_bool"])));
            Assert.That(obj.AnInt, Is.EqualTo(int.Parse(ConfigurationManager.AppSettings["some_int"])));
            Assert.That(obj.AString, Is.EqualTo(ConfigurationManager.AppSettings["some_string"]));
            Assert.That(obj.ADate, Is.EqualTo(DateTime.Parse(ConfigurationManager.AppSettings["some_date"])));
            return true;
        }

        public static bool IsCorrectlyConfigured(this SomethingWithNonPublicConfigurables obj)
        {
            Assert.That(obj.ABool, Is.EqualTo(bool.Parse(ConfigurationManager.AppSettings["some_bool"])));
            Assert.That(obj.AnInt, Is.EqualTo(int.Parse(ConfigurationManager.AppSettings["some_int"])));
            Assert.That(obj.AString, Is.EqualTo(ConfigurationManager.AppSettings["some_string"]));
            Assert.That(obj.ADate, Is.EqualTo(DateTime.Parse(ConfigurationManager.AppSettings["some_date"])));
            return true;
        }

        public static bool IsSameSequenceAs<T>(
            this IEnumerable<T> actual,
            params T[] expected)
        {
            return actual.IsSameSequenceAs((IEnumerable<T>) expected);
        }

        public static bool IsSameSequenceAs<T>(
            this IEnumerable<T> actual,
            IEnumerable<T> expected)
        {
            var actualArray = actual.ToArray();
            var expectedArray = expected.ToArray();

            try
            {
                Assert.That(actualArray.Length, Is.EqualTo(expectedArray.Length), "Sequences have different length");

                for (var i = 0; i < actualArray.Length; i++)
                {
                    if (!actualArray[i].Equals(expectedArray[i]))
                    {
                        Console.WriteLine("FAIL");
                        Assert.Fail(string.Format("Expected: {0}\nbut was: {1}",
                                                  expectedArray[i],
                                                  actualArray[i]));
                    }

                    Console.WriteLine("PASS ({0})", actualArray[i]);
                }
            }
            finally
            {
                Console.WriteLine(actualArray);
            }

            return true;
        }
    }
}