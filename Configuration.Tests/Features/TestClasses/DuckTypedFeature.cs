// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using Its.Configuration.Features;

namespace Its.Configuration.Tests.Features.TestClasses
{
    [Export(typeof (DuckTyped.IFeature))]
    public class DuckTypedFeature : DuckTyped.IFeature
    {
        public static int CtorCount = 0;
        public static int ActivateCount = 0;
        public static Action OnCtor = () => { };
        public static Action OnActivate = () => { };
        public static bool IsEnabled = true;

        public DuckTypedFeature()
        {
            Interlocked.Increment(ref CtorCount);
            OnCtor();
        }

        public IObservable<bool> Availability
        {
            get
            {
                return initialized;
            }
        }

        private readonly FeatureActivator initialized = new FeatureActivator(() =>
        {
            Interlocked.Increment(ref ActivateCount);
            OnActivate();
        });
    }

    public static class DuckTyped
    {
        public interface IFeature
        {
            IObservable<bool> Availability { get; }
        }
    }
}