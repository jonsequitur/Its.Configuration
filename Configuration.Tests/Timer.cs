// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Its.Configuration.Tests
{
    public static class Timer
    {
        public static TimeSpan TimeOperation(Action operation, int iterations)
        {
            GC.Collect();

            // warmup
            Thread.Sleep(2000);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < iterations; i++)
            {
                operation();
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}